using System;
using System.Collections.Generic;

namespace NostreetsExtensions.Interfaces
{
    public interface IDBService
    {
        List<object> GetAll();
        object Get(object id);
        object Insert(object model);
        void Delete(object id);
        void Update(object model);
        IEnumerable<object> Where(Func<object, bool> predicate);
    }


    public interface IDBService<T>
    {

        List<T> GetAll();
        T Get(object id);
        object Insert(T model);
        void Delete(object id);
        void Update(T model);
        IEnumerable<T> Where(Func<T, bool> predicate);
    }

    public interface IDBService<Type, IdType>
    {

        List<Type> GetAll();
        Type Get(IdType id);
        IdType Insert(Type model);
        void Delete(IdType id);
        void Update(Type model);
        IEnumerable<Type> Where(Func<Type, bool> predicate);
    }

    public interface IDBService<Type, IdType, AddType, UpdateType>
    {

        List<Type> GetAll();
        Type Get(IdType id);
        IdType Insert(AddType model);
        void Delete(IdType id);
        void Update(UpdateType model);
        IEnumerable<Type> Where(Func<Type, bool> predicate);
    }
}
