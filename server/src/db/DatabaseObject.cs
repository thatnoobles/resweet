using System.Collections.Generic;

namespace Resweet.Database;

public interface Entity
{
    public void BuildFromFields(object[] fields);

    public string ToJson();
}