using NetworkService.Helpers;
using System.ComponentModel;

namespace NetworkService.Model
{
    public class Entity : ValidationBase
    {
        private int? _id;
        private string _name;
        private EntityType _type;
        private double? _value;

        public int? Id
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

        public double? Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    OnPropertyChanged(nameof(Value));
                    OnPropertyChanged(nameof(IsInDanger));
                }
            }
        }

        public bool IsInDanger
        {
            get
            {
                if (Value == null) return false;
                return Value < 1 || Value > 5;
            }
        }

        protected override void ValidateSelf()
        {
            if (Id == null)
                ValidationErrors[nameof(Id)] = "ID is required.";
            else if (Id < 0)
                ValidationErrors[nameof(Id)] = "ID must be non-negative.";

            if (string.IsNullOrWhiteSpace(Name))
                ValidationErrors[nameof(Name)] = "Name is required.";

            if (Type == null)
                ValidationErrors[nameof(Type)] = "Type must be selected.";

            if (Value == null)
                ValidationErrors[nameof(Value)] = "Value is required.";
            else if (Value < 0 || Value > 10)
                ValidationErrors[nameof(Value)] = "Value must be in range 0-10.";
        }

        public override string ToString()
        {
            return Name;
        }
    }
}