//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Runtime.Serialization;
//using System.Runtime.Serialization.Json;
//using System.Text;
//using System.Threading.Tasks;
//using System.Xml;

//namespace Rpcsharp
//{
//    public static class RpcSerializer<T>
//    {
//        public static Func<T, string> InterfaceSerializer(Func<T, string> otherTypesSerializer)
//        {
            
//        }
//    }

//    public class RpcSerializer : XmlObjectSerializer
//    {
//        public override void WriteStartObject(XmlDictionaryWriter writer, object graph)
//        {
//            DataContractJsonSerializer j;

//        }

//        public override void WriteObjectContent(XmlDictionaryWriter writer, object graph)
//        {
//            throw new NotImplementedException();
//        }

//        public override void WriteEndObject(XmlDictionaryWriter writer)
//        {
//            throw new NotImplementedException();
//        }

//        public override object ReadObject(XmlDictionaryReader reader, bool verifyObjectName)
//        {
//            throw new NotImplementedException();
//        }

//        public override bool IsStartObject(XmlDictionaryReader reader)
//        {
//            throw new NotImplementedException();
//        }
//    }

//    public class DogResolver : DataContractResolver
//    {
//        public override bool TryResolveType(Type dataContractType, Type declaredType, DataContractResolver knownTypeResolver, out XmlDictionaryString typeName, out XmlDictionaryString typeNamespace)
//        {
//            if (typeof(IRpcRoot).IsAssignableFrom(dataContractType))
//            {
//                var dictionary = new XmlDictionary();
//                typeName = dictionary.Add("WOOF");
//                typeNamespace = dictionary.Add("http://www.myAnimals.com");
//                return true; // indicating that this resolver knows how to handle "Dog"
//            }
//            else
//            {
//                // Defer to the known type resolver
//                return knownTypeResolver.TryResolveType(dataContractType, declaredType, null, out typeName, out typeNamespace);
//            }
//        }

//        public override Type ResolveName(string typeName, string typeNamespace, Type declaredType, DataContractResolver knownTypeResolver)
//        {
//            if (typeName == "WOOF" && typeNamespace == "http://www.myAnimals.com")
//            {
//                return typeof(Dog);
//            }
//            else
//            {
//                // Defer to the known type resolver
//                return knownTypeResolver.ResolveName(typeName, typeNamespace, null);
//            }
//        }

//    }
//}
