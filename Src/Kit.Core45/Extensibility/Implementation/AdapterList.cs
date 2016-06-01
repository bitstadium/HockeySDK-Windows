namespace Microsoft.HockeyApp.Extensibility.Implementation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    
    /// <summary>
    /// A simple adapter list that can convert between two data types on the fly.
    /// </summary>
    /// <typeparam name="TPublic">The type of the public collection.</typeparam>
    /// <typeparam name="TPrivate">The type of the private collection.</typeparam>
    internal class AdapterList<TPublic, TPrivate> : 
        IList<TPublic>
    {
        /// <summary>
        /// The public collection.
        /// </summary>
        private readonly IList<TPublic> publicCollection;

        /// <summary>
        /// The private collection.
        /// </summary>
        private readonly IList<TPrivate> privateCollection;

        /// <summary>
        /// The convert public to private object delegate.
        /// </summary>
        private readonly Func<TPublic, TPrivate> convertPublicToPrivate;

        /// <summary>
        /// The convert private to public object delegate.
        /// </summary>
        private readonly Func<TPrivate, TPublic> convertPrivateToPublic;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdapterList{TPublic, TPrivate}"/> class.
        /// </summary>
        /// <param name="publicCollection">The public collection.</param>
        /// <param name="privateCollection">The private collection.</param>
        /// <param name="convertPublicToPrivate">The convert public to private object delegate.</param>
        /// <param name="convertPrivateToPublic">The convert private to public object delegate.</param>
        public AdapterList(IList<TPublic> publicCollection, IList<TPrivate> privateCollection, Func<TPublic, TPrivate> convertPublicToPrivate, Func<TPrivate, TPublic> convertPrivateToPublic)
        {
            if (publicCollection == null)
            {
                throw new ArgumentNullException("publicCollection");
            }

            if (privateCollection == null)
            {
                throw new ArgumentNullException("privateCollection");
            }

            if (convertPublicToPrivate == null)
            {
                throw new ArgumentNullException("convertPublicToPrivate");
            }

            if (convertPrivateToPublic == null)
            {
                throw new ArgumentNullException("convertPrivateToPublic");
            }

            this.publicCollection = publicCollection;
            this.privateCollection = privateCollection;

            this.convertPublicToPrivate = convertPublicToPrivate;
            this.convertPrivateToPublic = convertPrivateToPublic;
        }

        /// <summary>
        /// Gets the public collection.
        /// </summary>
        public IList<TPublic> PublicCollection
        {
            get { return this.publicCollection;  }
        }

        /// <summary>
        /// Gets the private collection.
        /// </summary>
        public IList<TPrivate> PrivateCollection
        {
            get { return this.privateCollection; }
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        public int Count
        {
            get { return this.publicCollection.Count; }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <returns>
        /// The element at the specified index.
        /// </returns>
        /// <param name="index">The zero-based index of the element to get or set.</param><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"/>.</exception><exception cref="T:System.NotSupportedException">The property is set and the <see cref="T:System.Collections.Generic.IList`1"/> is read-only.</exception>
        public TPublic this[int index]
        {
            get
            {
                return this.publicCollection[index];
            }

            set
            {
                this.privateCollection[index] = this.convertPublicToPrivate(value);
                this.publicCollection[index] = value;
            }
        }

        /// <summary>
        /// Synchronizes the public collection to the private collection.
        /// </summary>
        public void SyncPublicToPrivate()
        {
            this.privateCollection.Clear();
            foreach (TPublic item in this.publicCollection)
            {
                this.privateCollection.Add(this.convertPublicToPrivate(item));
            }
        }

        /// <summary>
        /// Synchronizes the private collection to the public collection.
        /// </summary>
        public void SyncPrivateToPublic()
        {
            this.publicCollection.Clear();
            foreach (TPrivate item in this.privateCollection)
            {
                this.publicCollection.Add(this.convertPrivateToPublic(item));
            }
        }

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.</exception>
        public void Add(TPublic item)
        {
            this.privateCollection.Add(this.convertPublicToPrivate(item));
            this.publicCollection.Add(item);
        }

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only. </exception>
        public void Clear()
        {
            this.privateCollection.Clear();
            this.publicCollection.Clear();
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"/> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        /// <returns>
        /// True if <paramref name="item"/> is found in the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false.
        /// </returns>
        public bool Contains(TPublic item)
        {
            return this.publicCollection.Contains(item);
        }

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1"/> to an <see cref="T:System.Array"/>, starting at a particular <see cref="T:System.Array"/> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1"/>. The <see cref="T:System.Array"/> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="array"/> is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="arrayIndex"/> is less than 0.</exception>
        /// <exception cref="T:System.ArgumentException">The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1"/> is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.</exception>
        public void CopyTo(TPublic[] array, int arrayIndex)
        {
            this.publicCollection.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        /// <returns>
        /// True if <paramref name="item"/> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false. This method also returns false if <paramref name="item"/> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.</exception>
        public bool Remove(TPublic item)
        {
            this.privateCollection.Remove(this.convertPublicToPrivate(item));
            return this.publicCollection.Remove(item);
        }

        /// <summary>
        /// Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1"/>.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1"/>.</param>
        /// <returns>
        /// The index of <paramref name="item"/> if found in the list; otherwise, -1.
        /// </returns>
        public int IndexOf(TPublic item)
        {
            return this.publicCollection.IndexOf(item);
        }

        /// <summary>
        /// Inserts an item to the <see cref="T:System.Collections.Generic.IList`1"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
        /// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1"/>.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"/>.</exception>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1"/> is read-only.</exception>
        public void Insert(int index, TPublic item)
        {
            this.privateCollection.Insert(index, this.convertPublicToPrivate(item));
            this.publicCollection.Insert(index, item);
        }

        /// <summary>
        /// Removes the <see cref="T:System.Collections.Generic.IList`1"/> item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"/>.</exception>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1"/> is read-only.</exception>
        public void RemoveAt(int index)
        {
            this.privateCollection.RemoveAt(index);
            this.publicCollection.RemoveAt(index);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<TPublic> GetEnumerator()
        {
            return this.publicCollection.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this.publicCollection).GetEnumerator();
        }
    }
}
