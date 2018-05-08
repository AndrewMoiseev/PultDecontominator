using System.Collections.Specialized;

namespace PultDecontominator.Models
{
    public class DecontaminatorRegister
    {
        public DecontaminatorRegister(string name, string panel, string typeData, int addresRegister, string description, string descriptionTypeData)
        {
            Name = name;
            Panel = panel;
            TypeData = typeData;
            AddresRegister = addresRegister;
            Description = description;
            DescriptionTypeData = descriptionTypeData;
        }

        public string Name
        {
            get ; 
            set ; 
        }
        public string Panel { get; }
        public string TypeData
        {
            get;
            set;
        }
        public int AddresRegister
        {
            get;
            set;
        }
        public string Description
        {
            get;
            set;
        }
        public string DescriptionTypeData
        {
            get;
            set;
        }
        public ushort RegisterValue
        {
            get;
            set;
        }

    }
}