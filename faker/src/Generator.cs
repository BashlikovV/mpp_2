using System.Collections;
using System.Reflection;

#pragma warning disable CS8602
#pragma warning disable CS0168

namespace faker {

public static class Generators
{
    private static Random random = new Random();

    private delegate object Generator(Type type);

    private static readonly Dictionary<Type, Generator> typeToGenerator = new Dictionary<Type, Generator>()
        {
            { typeof(int), generateInt},
            { typeof(float), generateFloat},
            { typeof(double), generateDouble},
            { typeof(long), generateLong},
            { typeof(byte), generateByte},
            { typeof(sbyte), generateSByte},
            { typeof(bool), generateBool},
            { typeof(uint), generateUInt},
            { typeof(ulong), generateULong},
            { typeof(decimal), generateDecimal},
            { typeof(char), generateChar},
            { typeof(object), generateObject},
            { typeof(string), generateString},
            { typeof(DateTime), generateDateTime},
            { typeof(IList), generateList},
        };


    private static object generate(Type type)
    {
        var ifaces = type.GetInterfaces();
        foreach (var iface in ifaces)
        {
            if (iface.Name.Contains("IList"))
            {
                return typeToGenerator[typeof(IList)](iface);
            }
        }
        return typeToGenerator[type](type);
    }

    public static object generateDto(Type type)
    {
        HashSet<Type> usedTypes = [];

        object generateRecursive(Type type, bool considerType)
        {
            if (considerType && !usedTypes.Add(type))
            {
                throw new Exception("Cyclic dependence");
            }
             
            var members = type.GetMembers();
            var privateFields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Where(field => !field.Name.Contains(">k__BackingField")).ToList();
            var privateProperties = type.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance).Concat(type.GetProperties().Where(prop => prop.SetMethod == null || prop.SetMethod != null && !prop.SetMethod.IsPublic).ToList()).ToList();
            var privateMembers = privateFields.Select(member => member.Name.ToLower()).ToList().Union(privateProperties.Select(member => member.Name.ToLower()).ToList()).ToList();
            int privateMembersMaxAmount = -1;

/* Находим самый большой конструктор */
            ConstructorInfo? savedConstructor = null;
            foreach (var constructor in type.GetConstructors())
            {
                int privateMembersAmount = 0;
                foreach (var parameter in constructor.GetParameters())
                {
                    if (privateMembers.Contains(parameter.Name!.ToLower()))
                    {
                        privateMembersAmount++;
                    }
                }
                if (constructor.IsPublic && privateMembersAmount > privateMembersMaxAmount)
                {
                    privateMembersMaxAmount = privateMembersAmount;
                    savedConstructor = constructor;
                }
            }
            if (savedConstructor == null)
            {
                throw new Exception("No public constructor");
            }

/* Генерим значения для всех переменных конструктора */
            List<object> parameters = [];
            foreach (var parameter in savedConstructor.GetParameters())
            {
                try
                {
                    parameters.Add(generate(parameter.ParameterType));
                }
                catch (KeyNotFoundException)
                {
                    parameters.Add(generateRecursive(parameter.ParameterType, true));
                }
            }

            var filledDto = savedConstructor.Invoke([.. parameters]);

            var publicMembers = type.GetMembers().Where(publicMember =>
            (publicMember.MemberType == MemberTypes.Field && (publicMember as FieldInfo).IsPublic) ||
            (publicMember.MemberType == MemberTypes.Property && (publicMember as PropertyInfo).SetMethod != null && (publicMember as PropertyInfo).SetMethod.IsPublic)).ToList();

            var erroredItems = new List<MemberInfo>();
            foreach (var publicMember in publicMembers)
            {
                try
                {
                    if (publicMember.MemberType == MemberTypes.Field)
                    {
                        var fieldInfo = publicMember as FieldInfo;
                        fieldInfo.SetValue(filledDto, generate(fieldInfo.FieldType));
                    }
                    else
                    {
                        var propertyInfo = publicMember as PropertyInfo;
                        propertyInfo.SetValue(filledDto, generate(propertyInfo.PropertyType));
                    }
                }
                catch (KeyNotFoundException)
                {
/* Ксли класс содержит переменные не бахзовых типов */
                    erroredItems.Add(publicMember);
                }
            }

            foreach (var erroredItem in erroredItems)
            {
                var potentialListType = erroredItem.MemberType == MemberTypes.Field ? (erroredItem as FieldInfo)!.FieldType : (erroredItem as PropertyInfo)!.PropertyType;
                var implementsList = potentialListType.GetInterface("IList`1") != null;

                if (implementsList)
                {
                    var listType = potentialListType.GetInterface("IList`1");
                    var genericType = listType.GenericTypeArguments[0];
                   
                    while (true)
                    {
                        var genericListType = genericType.GetInterface("IList`1");
                        
                        if (genericListType != null) {
                            break;
                        }

                        genericType = genericListType.GenericTypeArguments[0];
                    }

                    usedTypes.Add(genericType);

                    object generateRecursiveList(Type type)
                    {
                        object? obj = null;
                        
                        var listType = typeof(List<>).MakeGenericType(type.GenericTypeArguments[0]);
                        var listInstance = Activator.CreateInstance(listType);
                        var listContents = (IList)Convert.ChangeType(listInstance, listType)!;

                        for (int i = 0; i < 4; i++)
                        {
                            if (type.GenericTypeArguments[0].GetInterface("IList`1") != null)
                            {
                                obj = generateRecursiveList(type.GenericTypeArguments[0]);
                            }
                            else
                            {
                                obj = generateRecursive(type.GenericTypeArguments[0], false);
                            }
                            
                            Convert.ChangeType(obj, type.GenericTypeArguments[0]);

                            listContents.Add(obj);
                        }
                        return listContents;
                    }
                    
                    if (erroredItem.MemberType == MemberTypes.Field)
                    {
                        var fieldInfo = erroredItem as FieldInfo;
                        fieldInfo.SetValue(filledDto, generateRecursiveList(fieldInfo.FieldType));
                    }
                    else
                    {
                        var propertyInfo = erroredItem as PropertyInfo;
                        propertyInfo.SetValue(filledDto, generateRecursiveList(propertyInfo.PropertyType));
                    }
                }
                else
                {
                    if (erroredItem.MemberType == MemberTypes.Field)
                    {
                        var fieldInfo = erroredItem as FieldInfo;
                        fieldInfo.SetValue(filledDto, generateRecursive(fieldInfo.FieldType, true));
                    }
                    else
                    {
                        var propertyInfo = erroredItem as PropertyInfo;
                        propertyInfo.SetValue(filledDto, generateRecursive(propertyInfo.PropertyType, true));
                    }
                }
            }

            return filledDto;
        }

        if (type.Assembly.FullName!.Contains("System."))
        {
            return generate(type);
        }
        else
        {
            return generateRecursive(type, true);
        }
    }

    private static object generateInt(Type type) => random.Next();

    private static object generateFloat(Type type) => random.NextSingle();

    private static object generateDouble(Type type) => random.NextDouble();

    private static object generateLong(Type type) => random.NextInt64();

    private static object generateByte(Type type) => (byte)random.Next(0, 256);

    private static object generateSByte(Type type) => (sbyte)random.Next(sbyte.MinValue, sbyte.MaxValue + 1);

    private static object generateBool(Type type) => random.Next(2) == 0;

    private static object generateUInt(Type type) => (uint)random.Next();

    private static object generateULong(Type type) => (ulong)random.NextInt64();

    private static object generateDecimal(Type type) => (decimal)random.NextDouble();

    private static object generateChar(Type type) => (char)random.Next(char.MinValue, char.MaxValue + 1);

    private static object generateObject(Type type) => random.Next();

    private static object generateString(Type type)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        int length = random.Next(10, 20);
        return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
    }

    private static object generateDateTime(Type type)
    {
        return new DateTime(
            year: random.Next(0, DateTime.Now.Year + 1),
            month: random.Next(0, 13),
            day: random.Next(0, 28),
            hour: random.Next(0, 25),
            minute: random.Next(0, 61),
            second: random.Next(0, 61)
        );
    }

    private static object generateList(Type type)
    {
        var length = random.Next(3, 6);
        var listType = typeof(List<>).MakeGenericType(type.GenericTypeArguments[0]);
        var list = (IList)Convert.ChangeType(Activator.CreateInstance(listType), listType)!;
        for (var i = 0; i < length; i++)
        {
            object? obj = generate(type.GenericTypeArguments[0]);
            Convert.ChangeType(obj, type.GenericTypeArguments[0]);
            list.Add(obj);
        }
        return list;
    }
}

}