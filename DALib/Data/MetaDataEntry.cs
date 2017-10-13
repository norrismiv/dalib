using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DALib.Data
{
    public partial class MetaDataEntry
    {
        private string _key;
        private List<string> _values;
        private ReadOnlyCollection<string> _valuesReadOnly;

        internal MetaDataEntry(string key, IEnumerable<string> values)
        {
            _key = key;
            _values = new List<string>(values);
            _valuesReadOnly = new ReadOnlyCollection<string>(_values);
        }

        public string Key => _key;

        public ReadOnlyCollection<string> Values => _valuesReadOnly;

        public string this[int index] => _values[index];
    }

    public partial class MetaDataEntry : IEnumerable<string>
    {
        public IEnumerator<string> GetEnumerator() => ((IEnumerable<string>)_values).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<string>)_values).GetEnumerator();
    }
}
