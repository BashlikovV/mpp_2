

namespace faker {

    public class Program
    {
        public static void Main(){ 
            var faker = new FakerImpl();
            var filledObj = faker.create<FillableClass>();
            Console.WriteLine(filledObj.toString());
        }
    }

    class FillableClass(string str = "", int id = 0, char smth = 'c')
    {
        public string getStr() {
            return str;
        }

        public int getId()
        {
            return id;
        }

        public char getSmth()
        {
            return smth;
        }

        public string toString()
        {
            return "str: " + str + ", id: " + id + ", smth: " + smth + ";";
        }
    }

}