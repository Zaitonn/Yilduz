using System;
using SystemEncoding = System.Text.Encoding;

namespace Yilduz.Utils;

#pragma warning disable IDE0066

internal static class EncodingHelper
{
    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Encoding_API/Encodings
    /// </summary>
    public static string NormalizeEncodingName(string label)
    {
        switch (label.ToLowerInvariant())
        {
            case "unicode-1-1-utf-8"
            or "utf-8"
            or "utf8":
                return "utf-8";

            case "866"
            or "cp866"
            or "csibm866"
            or "ibm866":
                return "ibm866";

            case "csisolatin2"
            or "iso-8859-2"
            or "iso-ir-101"
            or "iso8859-2"
            or "iso88592"
            or "iso_8859-2"
            or "iso_8859-2:1987"
            or "l2"
            or "latin2":
                return "iso-8859-2";

            case "csisolatin3"
            or "iso-8859-3"
            or "iso-ir-109"
            or "iso8859-3"
            or "iso88593"
            or "iso_8859-3"
            or "iso_8859-3:1988"
            or "l3"
            or "latin3":
                return "iso-8859-3";

            case "csisolatin4"
            or "iso-8859-4"
            or "iso-ir-110"
            or "iso8859-4"
            or "iso88594"
            or "iso_8859-4"
            or "iso_8859-4:1988"
            or "l4"
            or "latin4":
                return "iso-8859-4";

            case "csisolatincyrillic"
            or "cyrillic"
            or "iso-8859-5"
            or "iso-ir-144"
            or "iso88595"
            or "iso_8859-5"
            or "iso_8859-5:1988":
                return "iso-8859-5";

            case "arabic"
            or "asmo-708"
            or "csiso88596e"
            or "csiso88596i"
            or "csisolatinarabic"
            or "ecma-114"
            or "iso-8859-6"
            or "iso-8859-6-e"
            or "iso-8859-6-i"
            or "iso-ir-127"
            or "iso8859-6"
            or "iso88596"
            or "iso_8859-6"
            or "iso_8859-6:1987":
                return "iso-8859-6";

            case "csisolatingreek"
            or "ecma-118"
            or "elot_928"
            or "greek"
            or "greek8"
            or "iso-8859-7"
            or "iso-ir-126"
            or "iso8859-7"
            or "iso88597"
            or "iso_8859-7"
            or "iso_8859-7:1987"
            or "sun_eu_greek":
                return "iso-8859-7";

            case "csiso88598e"
            or "csisolatinhebrew"
            or "hebrew"
            or "iso-8859-8"
            or "iso-8859-8-e"
            or "iso-ir-138"
            or "iso8859-8"
            or "iso88598"
            or "iso_8859-8"
            or "iso_8859-8:1988"
            or "visual":
                return "iso-8859-8";

            case "csiso88598i"
            or "iso-8859-8-i"
            or "logical":
                return "iso-8859-8i";

            case "csisolatin6"
            or "iso-8859-10"
            or "iso-ir-157"
            or "iso8859-10"
            or "iso885910"
            or "l6"
            or "latin6":
                return "iso-8859-10";

            case "iso-8859-13"
            or "iso8859-13"
            or "iso885913":
                return "iso-8859-13";

            case "iso-8859-14"
            or "iso8859-14"
            or "iso885914":
                return "iso-8859-14";

            case "csisolatin9"
            or "iso-8859-15"
            or "iso8859-15"
            or "iso885915"
            or "l9"
            or "latin9":
                return "iso-8859-15";

            case "iso-8859-16":
                return "iso-8859-16";

            case "cskoi8r"
            or "koi"
            or "koi8"
            or "koi8-r"
            or "koi8_r":
                return "koi8-r";

            case "koi8-u":
                return "koi8-u";

            case "csmacintosh"
            or "mac"
            or "macintosh"
            or "x-mac-roman":
                return "macintosh";

            case "dos-874"
            or "iso-8859-11"
            or "iso8859-11"
            or "iso885911"
            or "tis-620"
            or "windows-874":
                return "windows-874";

            case "cp1250"
            or "windows-1250"
            or "x-cp1250":
                return "windows-1250";

            case "cp1251"
            or "windows-1251"
            or "x-cp1251":
                return "windows-1251";

            case "ansi_x3.4-1968"
            or "ascii"
            or "cp1252"
            or "cp819"
            or "csisolatin1"
            or "ibm819"
            or "iso-8859-1"
            or "iso-ir-100"
            or "iso8859-1"
            or "iso88591"
            or "iso_8859-1"
            or "iso_8859-1:1987"
            or "l1"
            or "latin1"
            or "us-ascii"
            or "windows-1252"
            or "x-cp1252":
                return "windows-1252";

            case "cp1253"
            or "windows-1253"
            or "x-cp1253":
                return "windows-1253";

            case "cp1254"
            or "csisolatin5"
            or "iso-8859-9"
            or "iso-ir-148"
            or "iso8859-9"
            or "iso88599"
            or "iso_8859-9"
            or "iso_8859-9:1989"
            or "l5"
            or "latin5"
            or "windows-1254"
            or "x-cp1254":
                return "windows-1254";

            case "cp1255"
            or "windows-1255"
            or "x-cp1255":
                return "windows-1255";

            case "cp1256"
            or "windows-1256"
            or "x-cp1256":
                return "windows-1256";

            case "cp1257"
            or "windows-1257"
            or "x-cp1257":
                return "windows-1257";

            case "cp1258"
            or "windows-1258"
            or "x-cp1258":
                return "windows-1258";

            case "x-mac-cyrillic"
            or "x-mac-ukrainian":
                return "x-mac-cyrillic";

            case "chinese"
            or "csgb2312"
            or "csiso58gb231280"
            or "gb2312"
            or "gb_2312"
            or "gb_2312-80"
            or "gbk"
            or "iso-ir-58"
            or "x-gbk":
                return "gbk";

            case "gb18030":
                return "gb18030";

            case "hz-gb-2312":
                return "hz-gb-2312";

            case "big5"
            or "big5-hkscs"
            or "cn-big5"
            or "csbig5"
            or "x-x-big5":
                return "big5";

            case "cseucpkdfmtjapanese"
            or "euc-jp"
            or "x-euc-jp":
                return "euc-jp";

            case "csiso2022jp"
            or "iso-2022-jp":
                return "iso-2022-jp";

            case "csshiftjis"
            or "ms_kanji"
            or "shift-jis"
            or "shift_jis"
            or "sjis"
            or "windows-31j"
            or "x-sjis":
                return "shift-jis";

            case "cseuckr"
            or "csksc56011987"
            or "euc-kr"
            or "iso-ir-149"
            or "korean"
            or "ks_c_5601-1987"
            or "ks_c_5601-1989"
            or "ksc5601"
            or "ksc_5601"
            or "windows-949":
                return "euc-kr";

            case "csiso2022kr"
            or "iso-2022-kr":
                return "iso-2022-kr";

            case "utf-16be":
                return "utf-16be";

            case "utf-16"
            or "utf-16le":
                return "utf-16le";

            default:
                throw new ArgumentOutOfRangeException(nameof(label));
        }
    }
}
