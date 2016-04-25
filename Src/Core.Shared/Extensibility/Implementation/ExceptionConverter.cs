namespace Microsoft.HockeyApp.Extensibility.Implementation
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    internal static class ExceptionConverter
    {
        public const int MaxParsedStackLength = 32768;

        /// <summary>
        /// Converts a System.Exception to a ExceptionDetails.
        /// </summary>
        internal static External.ExceptionDetails ConvertToExceptionDetails(
            Exception exception,
            External.ExceptionDetails parentExceptionDetails)
        {
            External.ExceptionDetails exceptionDetails = External.ExceptionDetails.CreateWithoutStackInfo(
                                                                                                                exception,
                                                                                                                parentExceptionDetails);
            if (exception.StackTrace != null)
            {
                string[] lines = exception.StackTrace.Split(new string[] { "\n" }, StringSplitOptions.None);

                // Adding 1 for length in lengthGetter for newline character
                Tuple<List<string>, bool> sanitizedTuple = SanitizeStackFrame(
                                                                            lines,
                                                                            (input, id) => input,
                                                                            (input) => input == null ? 0 : input.Length + 1);
                List<string> sanitizedStackLines = sanitizedTuple.Item1;
                exceptionDetails.hasFullStack = sanitizedTuple.Item2;
                exceptionDetails.stack = string.Join("\n", sanitizedStackLines.ToArray());
            }
            else
            {
                exceptionDetails.hasFullStack = true;
                exceptionDetails.stack = string.Empty;
            }

            return exceptionDetails;
        }

        /// <summary>
        /// Sanitizing stack to 32k while selecting the initial and end stack trace.
        /// </summary>
        private static Tuple<List<TOutput>, bool> SanitizeStackFrame<TInput, TOutput>(
            IList<TInput> inputList,
            Func<TInput, int, TOutput> converter,
            Func<TOutput, int> lengthGetter)
        {
            List<TOutput> orderedStackTrace = new List<TOutput>();
            bool hasFullStack = true;
            if (inputList != null && inputList.Count > 0)
            {
                int currentParsedStackLength = 0;
                for (int level = 0; level < inputList.Count; level++)
                {
                    // Skip middle part of the stack
                    int current = (level % 2 == 0) ? (inputList.Count - 1 - (level / 2)) : (level / 2);

                    TOutput convertedStackFrame = converter(inputList[current], current);
                    currentParsedStackLength += lengthGetter(convertedStackFrame);

                    if (currentParsedStackLength > ExceptionConverter.MaxParsedStackLength)
                    {
                        hasFullStack = false;
                        break;
                    }

                    orderedStackTrace.Insert(orderedStackTrace.Count / 2, convertedStackFrame);
                }
            }

            return new Tuple<List<TOutput>, bool>(orderedStackTrace, hasFullStack);
        }
    }
}