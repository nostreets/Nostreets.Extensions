using System;
using System.Collections.Generic;

namespace NostreetsExtensions.Interfaces
{
    public interface IDBService<Type, IdType, AddType, UpdateType>
    {

        List<Type> GetAll();
        Type Get(IdType id);
        IdType Insert(AddType model);
        void Delete(IdType id);
        void Update(UpdateType model);
    }

    public interface IDBService<T>
    {

        List<T> GetAll();
        T Get(object id);
        object Insert(T model);
        void Delete(object id);
        void Update(T model);
    }

    public interface IDBService<Type, IdType>
    {

        List<Type> GetAll();
        Type Get(IdType id);
        IdType Insert(Type model);
        void Delete(IdType id);
        void Update(Type model);
    }

    
}
