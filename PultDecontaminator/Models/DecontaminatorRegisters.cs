using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using PultDecontominator.Annotations;

namespace PultDecontominator.Models
{
    public class DecontaminatorRegister : INotifyPropertyChanged
    {

        public DecontaminatorRegister(string name, string panel, string typeData, ushort addresRegister, string description, string descriptionTypeData)
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
        public ushort AddresRegister
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
            get => _registerValue;
            set
            {
                _registerValue = value;
                OnPropertyChanged("RegisterValue");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        private ushort _registerValue;

        //[NotifyPropertyChangedInvocator]
        //protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        //{
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        //}
    }
}