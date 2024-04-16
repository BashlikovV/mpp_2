

namespace faker {

    public class Program
    {
        public static void Main(){ 
            var faker = new FakerImpl();
            var filledObj = faker.create<FillableClass>();
            Console.WriteLine(filledObj.getStr());
        }
    }

    class FillableClass(string str = "")
    {
        public string getStr() {
            return str;
        }
    }

}