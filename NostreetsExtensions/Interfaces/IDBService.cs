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

    public interface IDBService<T, IdType>
    {

        List<T> GetAll();
        T Get(IdType id);
        IdType Insert(T model);
        void Delete(IdType id);
        void Update(T model);
        IEnumerable<T> Where(Func<T, bool> predicate);
    }

    public interface IDBService<T, IdType, AddType, UpdateType>
    {

        List<T> GetAll();
        T Get(IdType id);
        IdType Insert(T model);
        IdType Insert(AddType model, Converter<AddType, T> converter);
        void Delete(IdType id);
        void Update(UpdateType model, Converter<UpdateType, T> converter);
        void Update(T model);
        IEnumerable<T> Where(Func<T, bool> predicate);
    }
}
