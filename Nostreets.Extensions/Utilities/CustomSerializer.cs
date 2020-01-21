﻿using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RestSharp;
using RestSharp.Deserializers;
using RestSharp.Serializers;
using System;
using System.IO;
using System.Linq;

namespace Nostreets.Extensions.Utilities
{
    public class CustomSerializer : ISerializer, IDeserializer
    {
        public CustomSerializer(Newtonsoft.Json.JsonSerializer serializer)
        {
            _serialize = serializer;
        }

        Newtonsoft.Json.JsonSerializer _serialize;

        public string ContentType
        {
            get { return "application/json"; }
            set { }
        }

        public string DateFormat { get; set; }

        public string Namespace { get; set; }

        public string RootElement { get; set; }

        public static CustomSerializer CamelCase
        {
            get
            {
                return new CustomSerializer(new Newtonsoft.Json.JsonSerializer { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            }
        }

        public static CustomSerializer CamelCaseIngoreDictionaryKeys
        {
            get
            {
                return new CustomSerializer(new Newtonsoft.Json.JsonSerializer { ContractResolver = new CamelCaseIngoreDictionaryKeysResolver() });
            }
        }

        public string Serialize(object obj)
        {
            using (var stringWriter = new StringWriter())
            {
                using (var jsonTextWriter = new JsonTextWriter(stringWriter))
                {
                    _serialize.Serialize(jsonTextWriter, obj);

                    return stringWriter.ToString();
                }
            }
        }

        public T Deserialize<T>(IRestResponse response)
        {
            var content = response.Content;

            using (var stringReader = new StringReader(content))
            {
                using (var jsonTextReader = new JsonTextReader(stringReader))
                {
                    return _serialize.Deserialize<T>(jsonTextReader);
                }
            }
        }
    }


    class CamelCaseIngoreDictionaryKeysResolver : CamelCasePropertyNamesContractResolver
    {
        protected override JsonDictionaryContract CreateDictionaryContract(Type objectType)
        {
            JsonDictionaryContract contract = base.CreateDictionaryContract(objectType);

            contract.DictionaryKeyResolver = propertyName => propertyName;

            return contract;
        }
    }
}