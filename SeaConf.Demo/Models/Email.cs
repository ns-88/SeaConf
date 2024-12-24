namespace SeaConf.Demo.Models
{
    internal class Email
    {
        public readonly string Value;

        public Email(string value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value;
        }
    }
}