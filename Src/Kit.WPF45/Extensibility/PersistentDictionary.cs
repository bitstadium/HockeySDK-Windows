namespace Microsoft.HockeyApp.Extensibility.Implementation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.IsolatedStorage;
    using System.Linq;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// This persistent dictionary makes use of the isolated storage, BinaryFormatter serialization
    /// </summary>
    public class PersistentDictionary<T, U> : IDictionary<T, U>
    {
        /// <summary>
        /// The synchronize root
        /// </summary>
        private object syncRoot = new object();

        /// <summary>
        /// The isolated storage
        /// </summary>
        private IsolatedStorageFile isolatedStorage = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);

        /// <summary>
        /// The filename
        /// </summary>
        private string filename;

        /// <summary>
        /// The internal dictionary
        /// </summary>
        private Dictionary<T, U> internalDictionary;

        /// <summary>
        /// The initialized flag
        /// </summary>
        private bool initialized = false;

        /// <summary>
        /// The formatter
        /// </summary>
        private BinaryFormatter formatter = new BinaryFormatter();

        /// <summary>
        /// Initializes a new instance of the <see cref="PersistentDictionary{T, U}"/> class.
        /// </summary>
        /// <param name="filename">The filename.</param>
        public PersistentDictionary(string filename)
        {
            this.filename = filename;
        }

        /// <summary>
        /// Ensures the current instance has been initialized.
        /// </summary>
        protected void EnsureInitialized()
        {
            if (!initialized)
            {
                lock (syncRoot)
                {
                    if (!initialized)
                    {
                        try
                        {
                            using (var stream = this.isolatedStorage.OpenFile(filename, FileMode.Open, FileAccess.Read))
                            {
                                this.internalDictionary = (Dictionary<T, U>)formatter.Deserialize(stream);
                            }
                        }
                        catch (Exception e)
                        {
                            // At least log to the default output
                            HockeyClient.Current.AsInternal().HandleInternalUnhandledException(e);
                        }
                        finally
                        {
                            // Make sure that subsequent calls won't fail
                            if (this.internalDictionary == null)
                                this.internalDictionary = new Dictionary<T, U>();

                            initialized = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Saves this instance.
        /// </summary>
        protected void Save()
        {
            lock (syncRoot)
            {
                try
                {
                    using (var stream = isolatedStorage.OpenFile(filename, FileMode.Create, FileAccess.ReadWrite))
                    {
                        formatter.Serialize(stream, internalDictionary);
                    }
                }
                catch (Exception e)
                {
                    HockeyClient.Current.AsInternal().HandleInternalUnhandledException(e);
                }
            }
        }

        /// <summary>
        /// Adds an element with the provided key and value to the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        /// <param name="key">The object to use as the key of the element to add.</param>
        /// <param name="value">The object to use as the value of the element to add.</param>
        public void Add(T key, U value)
        {
            this.EnsureInitialized();
            this.internalDictionary.Add(key, value);
            this.Save();
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the <see cref="T:System.Collections.Generic.IDictionary`2" />.</param>
        /// <returns>
        /// true if the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the key; otherwise, false.
        /// </returns>
        public bool ContainsKey(T key)
        {
            this.EnsureInitialized();
            return this.internalDictionary.ContainsKey(key);
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        public ICollection<T> Keys
        {
            get { this.EnsureInitialized(); return this.internalDictionary.Keys; }
        }

        /// <summary>
        /// Removes the element with the specified key from the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns>
        /// true if the element is successfully removed; otherwise, false.  This method also returns false if <paramref name="key" /> was not found in the original <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </returns>
        public bool Remove(T key)
        {
            this.EnsureInitialized();
            var removed = this.internalDictionary.Remove(key);
            if (removed) this.Save();
            return removed;
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value" /> parameter. This parameter is passed uninitialized.</param>
        /// <returns>
        /// true if the object that implements <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the specified key; otherwise, false.
        /// </returns>
        public bool TryGetValue(T key, out U value)
        {
            this.EnsureInitialized();
            return this.internalDictionary.TryGetValue(key, out value);
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        public ICollection<U> Values
        {
            get { this.EnsureInitialized(); return this.internalDictionary.Values; }
        }

        /// <summary>
        /// Gets or sets the element with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public U this[T key]
        {
            get
            {
                this.EnsureInitialized();
                return this.internalDictionary[key];
            }
            set
            {
                this.EnsureInitialized();
                this.internalDictionary[key] = value;
                this.Save();
            }
        }

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void Add(KeyValuePair<T, U> item)
        {
            if (item.Key == null)
                return;

            this.EnsureInitialized();
            this.internalDictionary.Add(item.Key, item.Value);
            this.Save();
        }

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        public void Clear()
        {
            this.EnsureInitialized();

            if (this.internalDictionary.Count == 0)
                return;

            this.internalDictionary.Clear();
            this.Save();
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>
        /// true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public bool Contains(KeyValuePair<T, U> item)
        {
            this.EnsureInitialized();
            return this.internalDictionary.ContainsKey(item.Key) && this.internalDictionary.ContainsValue(item.Value);
        }

        /// <summary>
        /// Copies to.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="arrayIndex">Index of the array.</param>
        public void CopyTo(KeyValuePair<T, U>[] array, int arrayIndex)
        {
            // TODO: Not implemented
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        public int Count
        {
            get { this.EnsureInitialized(); return this.internalDictionary.Count; }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>
        /// true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </returns>
        public bool Remove(KeyValuePair<T, U> item)
        {
            // TODO: Not implemented
            return false;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<KeyValuePair<T, U>> GetEnumerator()
        {
            this.EnsureInitialized();
            return this.internalDictionary.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            this.EnsureInitialized();
            return ((IEnumerable)this.internalDictionary).GetEnumerator();
        }
    }
}
