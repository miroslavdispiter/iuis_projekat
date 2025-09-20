using NetworkService.Helpers;
using System.ComponentModel;

namespace NetworkService.Model
{
    public class Entity : ValidationBase
    {
        private int _id;
        private string _name;
        private EntityType _type;
        private double _value;

        public int Id
        {
            get => _id;
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged(nameof(Id));
                }
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public EntityType Type
        {
            get => _type;
            set
            {
                if (_type != value)
                {
                    _type = value;
                    OnPropertyChanged(nameof(Type));
                }
            }
        }

        public double Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    OnPropertyChanged(nameof(Value));
                }
            }
        }

        protected override void ValidateSelf()
        {
            if (Id < 0)
                ValidationErrors[nameof(Id)] = "ID must be non-negative.";

            if (string.IsNullOrWhiteSpace(Name))
                ValidationErrors[nameof(Name)] = "Name is required.";

            if (Type == null)
                ValidationErrors[nameof(Type)] = "Type must be selected.";

            if (Value < 0)
                ValidationErrors[nameof(Value)] = "Value must be positive.";
        }
    }
}