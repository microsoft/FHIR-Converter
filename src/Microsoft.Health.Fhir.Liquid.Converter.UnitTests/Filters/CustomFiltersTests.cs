using System.Collections.Generic;
using Xunit;

namespace Microsoft.Health.Fhir.Liquid.Converter.UnitTests;

public class CustomFilterTests
{
  [Fact]
  public void ToHtmlString_String_ReturnsSameString()
  {
    var actual = Filters.ToHtmlString("Doc");
    Assert.Equal("Doc", actual);
  }

  [Fact]
  public void ToHtmlString_ObjectList_ReturnsStrings()
  {
    var strList = new List<object>() { "Race", "car" };
    var actual = Filters.ToHtmlString(strList);
    Assert.Equal("Racecar", actual);
  }

  [Fact]
  public void ToHtmlString_StringObjectDictionaryUnderscore_ReturnsOnlyUnderscoreString()
  {
    var underscoreDict = new Dictionary<string, object>()
    {
            { "_", "car" },
            { "/nSun", "flower" },
    };
    var actual = Filters.ToHtmlString(underscoreDict);
    Assert.Equal("car", actual);
  }

  [Fact]
  public void ToHtmlString_StringObjectDictionaryBr_ReturnsOnlyBR()
  {
    var brDict = new Dictionary<string, object>()
    {
            { "br", string.Empty },
            { "/nSun", "flower" },
    };
    var actual = Filters.ToHtmlString(brDict);
    Assert.Equal("<br>", actual);
  }

  [Fact]
  public void ToHtmlString_StringObjectDictionaryAnotherDictionary_ReturnsHTMLValues()
  {
    var underscoreDict = new Dictionary<string, object>()
    {
            { "_", "paragraph text" },
    };
    var dictDict = new Dictionary<string, object>()
    {
            { "p", underscoreDict },
            { "/nSun", "flower" },
    };
    var actual = Filters.ToHtmlString(dictDict);
    Assert.Equal("<p>paragraph text</p>", actual);
  }

  [Fact]
  public void ToHtmlString_StringObjectDictionaryList_ReturnsEachItemWithTags()
  {
    var strList = new List<object>() { "Race", "car" };
    var dictDict = new Dictionary<string, object>()
    {
            { "span", strList },
            { "/nSun", "flower" },
    };
    var actual = Filters.ToHtmlString(dictDict);
    Assert.Equal("<span>Race</span><span>car</span>", actual);
  }

  [Fact]
  public void ToHtmlString_ComplicatedExample_ReturnsString()
  {
    var footnote = new Dictionary<string, object>()
    {
            { "ID", "subTitle11" },
            { "styleCode", "xSectionSubTitle" },
            { "_", "documented as of this encounter (statuses as of 07/25/2022)" },
    };
    var tbodyTrTd12Column1 = new Dictionary<string, object>()
    {
            { "ID", "problem12name" },
            { "_", "Essential hypertension" },
    };
    var tbodyTrTd12 = new List<object>() { tbodyTrTd12Column1, "7/21/22" };
    var tbodyTr12 = new Dictionary<string, object>()
    {
            { "ID", "problem12" },
            { "styleCode", "xRowAlt" },
            { "td", tbodyTrTd12 },
    };
    var tbodyTrTd13Column1 = new Dictionary<string, object>()
    {
            { "ID", "problem13name" },
            { "_", "Parkinson's syndrome" },
    };
    var tbodyTrTd13 = new List<object>() { tbodyTrTd13Column1, "7/25/22" };
    var tbodyTr13 = new Dictionary<string, object>()
    {
            { "ID", "problem13" },
            { "styleCode", "xRowNormal" },
            { "td", tbodyTrTd13 },
    };
    var tbodyTrList = new List<object>() { tbodyTr13, tbodyTr12 };
    var tbody = new Dictionary<string, object>()
    {
            { "tr", tbodyTrList },
    };
    var theadTrTh = new List<object>() { "Active Problems", "Noted Date" };
    var theadTr = new Dictionary<string, object>()
    {
            { "th", theadTrTh },
    };
    var thead = new Dictionary<string, object>()
    {
            { "tr", theadTr },
    };
    var col = new Dictionary<string, object>()
    {
            { "width", "50%" },
    };
    var colList = new List<object>() { col, col };
    var colGroup = new Dictionary<string, object>()
    {
            { "col", colList },
    };
    var table = new Dictionary<string, object>()
    {
            { "colgroup", colGroup },
            { "thead", thead },
            { "tbody", tbody },
    };
    var completeDict = new Dictionary<string, object>()
    {
            { "table", table },
            { "footnote", footnote },
    };

    var actual = Filters.ToHtmlString(completeDict);
    Assert.Equal("<table><thead><tr><th>Active Problems</th><th>Noted Date</th></tr></thead><tbody><tr><!-- data-id: problem13 --><td><!-- data-id: problem13name -->Parkinson's syndrome</td><td>7/25/22</td></tr><tr><!-- data-id: problem12 --><td><!-- data-id: problem12name -->Essential hypertension</td><td>7/21/22</td></tr></tbody></table>documented as of this encounter (statuses as of 07/25/2022)", actual);
  }

  [Fact]
  public void ToHtmlString_ContainsListXmlTags_ReturnsReplacedTags()
  {
    var itemList = new List<object>()
    {
                "Recurrent GI bleed of unknown etiology; hypotension perhaps secondary to this but as likely secondary to polypharmacy.",
                "Acute on chronic anemia secondary to #1.",
                "Azotemia, acute renal failure with volume loss secondary to #1.",
                "Hyperkalemia secondary to #3 and on ACE and K+ supplement.",
                "Other chronic diagnoses as noted above, currently stable.",
    };
    var list = new Dictionary<string, object>()
    {
                { "listType", "ordered" },
                { "item", itemList },
    };
    var complete = new Dictionary<string, object>()
    {
                { "list", list },
    };
    var actual = Filters.ToHtmlString(complete);
    Assert.Equal("<ul><li>Recurrent GI bleed of unknown etiology; hypotension perhaps secondary to this but as likely secondary to polypharmacy.</li><li>Acute on chronic anemia secondary to #1.</li><li>Azotemia, acute renal failure with volume loss secondary to #1.</li><li>Hyperkalemia secondary to #3 and on ACE and K+ supplement.</li><li>Other chronic diagnoses as noted above, currently stable.</li></ul>", actual);
  }

  [Fact]
  public void ToHtmlString_InvalidTags_ReturnsStringWithSpaces()
  {
    var raceString = new Dictionary<string, object>()
    {
                { "_", "two" },
    };
    var carString = new Dictionary<string, object>()
    {
                { "_", "words" },
    };
    var complete = new Dictionary<string, object>()
    {
                { "invalidTag", raceString },
                { "badTag", carString },
    };
    var actual = Filters.ToHtmlString(complete);
    Assert.Equal("two words", actual);
  }

  [Fact]
  public void GetLoincName_ValidLOINC_ReturnsName()
  {
    var loinc = "34565-2";
    var actual = Filters.GetLoincName(loinc);
    Assert.Equal("Vital signs, weight and height panel", actual);
  }

  [Fact]
  public void GetLoincName_InvalidLOINC_ReturnsNull()
  {
    var loinc = "ABC";
    var actual = Filters.GetLoincName(loinc);
    Assert.Null(actual);
  }

  [Fact]
  public void GetLoincName_Null_ReturnsNull()
  {
    var actual = Filters.GetLoincName(null);
    Assert.Null(actual);
  }

  [Fact]
  public void GetRxnormName_ValidRxnorm_ReturnsName()
  {
    var rxnorm = "1044916";
    var actual = Filters.GetRxnormName(rxnorm);
    Assert.Equal("VioNex", actual);
  }

  [Fact]
  public void GetRxnormName_InvalidRxnorm_ReturnsNull()
  {
    var rxnorm = "ABC";
    var actual = Filters.GetRxnormName(rxnorm);
    Assert.Null(actual);
  }

  [Fact]
  public void GetRxnormName_Null_ReturnsNull()
  {
    var actual = Filters.GetRxnormName(null);
    Assert.Null(actual);
  }

  [Fact]
  public void FindObjectByIdRecursive_ValidId_ReturnsObject()
  {
    var text1 = new List<object>()
    {
            "Correct return value",
    };
    var content1 = new Dictionary<string, object>()
    {
            { "ID", "test-id-1" },
            { "_", text1 },
    };
    var dict1 = new Dictionary<string, object>()
    {
            { "content", content1 },
    };
    var text2 = new List<object>()
    {
            "Incorrect return value",
    };
    var content2 = new Dictionary<string, object>()
    {
            { "ID", "test-id-2" },
            { "_", text2 },
    };
    var dict2 = new Dictionary<string, object>()
    {
            { "content", content2 },
    };
    var data = new List<object>()
    {
            dict1, dict2,
    };
    var actual = Filters.FindObjectById(data, "test-id-1");
    Assert.Equal(content1, actual);
  }

  [Fact]
  public void FindObjectById_NullData_ReturnsNull()
  {
    // var data = null;
    var actual = Filters.FindObjectById(null, "fake-id");
    Assert.Null(actual);
  }

  [Fact]
  public void FindInnerTextById_ValidId_ReturnsString()
  {
    var innerXml = "<paragraph style=\"bold\">hello</paragraph>";
    var fragment = $"<content ID=\"hi\">{innerXml}</content><content ID=\"bye\">bye</content>";
    var actual = Filters.FindInnerTextById(fragment, "hi");
    Assert.Equal(actual, innerXml);
  }

  [Fact]
  public void FindInnerTextById_InalidId_ReturnsNull()
  {

    var innerXml = "<paragraph style=\"bold\">hello</paragraph>";
    var fragment = $"<content ID=\"hi\">{innerXml}</content><content ID=\"bye\">bye</content>";
    var actual = Filters.FindInnerTextById(fragment, "nope");
    Assert.Null(actual);
  }

  [Fact]
  public void FindInnerTextById_Null_ReturnsNull()
  {
    var actual = Filters.FindInnerTextById(null, "nope");
    Assert.Null(actual);
  }

  [Fact]
  public void ConcatStrings_ValidData_ReturnsCorrectString()
  {
    var object1 = new List<object>()
    {
            "string2", "string1",
    };
    var object2 = new List<object>()
    {
            new Dictionary<string, object>() { { "key1", "string1" } },
            new Dictionary<string, object>() { { "key2", "string2"} },
    };
    var object3 = new Dictionary<string, object>() { { "key1", "string1" } };
    var object4 = new Dictionary<string, object>()
    {
      { "key1", "string1" },
      {
        "nestedDict", new Dictionary<string, object>()
        {
          { "key2", "string2" },
          { "key3", "string3" },
        }
      },
      { "key4", "string4" },
    };
    var object5 = new Dictionary<string, object>() {
      { "key1", "string1" },
      { "nestedList", new List<object>()
        {
            new Dictionary<string, object>() { { "key2", "string2" } },
            new Dictionary<string, object>() { { "key3", "string3" } },
        }
      },
      { "key4", "string4" },
    };

    var actual1 = Filters.ConcatStrings(object1);
    Assert.Equal("string1<br/>string2", actual1);

    var actual2 = Filters.ConcatStrings(object2);
    Assert.Equal("string1<br/>string2", actual2);

    var actual3 = Filters.ConcatStrings(object3);
    Assert.Equal("string1", actual3);

    var actual4 = Filters.ConcatStrings(object4);
    Assert.Equal("string1<br/>string2<br/>string3<br/>string4", actual4);

    var actual5 = Filters.ConcatStrings(object5);
    Assert.Equal("string1<br/>string3<br/>string2<br/>string4", actual5);
  }

  [Fact]
  public void ConcatStrings_NullData_ReturnsEmptyString()
  {
    var actual = Filters.ConcatStrings(null);
    Assert.Equal(string.Empty, actual);
  }

  [Fact]
  public void ConcatenateTds_EmptyData_ReturnsEmptyString()
  {
    var inputData = new Dictionary<string, object>();
    var actual = Filters.ConcatenateTds(inputData);

    Assert.Equal(string.Empty, actual);
  }

  [Theory]
  [ClassData(typeof(CustomFilterTestFixtures))]
  public void ConcatenateTds_Table_With_Thead_ReturnsReasons(IDictionary<string, object> inputData, string expected)
  {
    var actual = Filters.ConcatenateTds(inputData);
    Assert.Equal(expected, actual);
  }

  [Fact]
  public void GetDiagnosisDictionary_ReturnsDiagnosisDictionary()
  {
    var actual = Microsoft.Health.Fhir.Liquid.Converter.Filters.GetDiagnosisDictionary(CustomFilterTestFixtures.EncounterDiagnoses);
    Assert.Equal(new Dictionary<string, bool>() { { "B05.9", true }, { "B06.0", true }, { "B06.1", true } }, actual);
  }
}
