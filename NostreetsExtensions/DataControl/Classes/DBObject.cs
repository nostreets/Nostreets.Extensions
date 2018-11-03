using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using NostreetsExtensions.Extend.Basic;
using NostreetsExtensions.Extend.IOC;
using NostreetsExtensions.Utilities;

namespace NostreetsExtensions.DataControl.Classes
{
    public abstract partial class DBObject<T>
    {

        private DateTime _dateCreated = DateTime.Now;
        private DateTime _dateModified = DateTime.Now;
        private bool _isDeleted = false;
        private static bool _hasUserId = true;

        [Key]
        public T Id { get; set; }

        public virtual string UserId { get; set; }

        public virtual DateTime? DateCreated { get => _dateCreated; set => _dateCreated = value.Value; }

        public virtual DateTime? DateModified { get => _dateModified; set => _dateModified = value.Value; }

        public virtual string ModifiedUserId { get; set; }

        public virtual bool IsDeleted { get => _isDeleted; set => _isDeleted = value; }

    }

    public abstract class DBObject : DBObject<int>
    {

    }




}
