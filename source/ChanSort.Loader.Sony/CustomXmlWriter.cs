using System;
using System.IO;
using System.Xml;

namespace ChanSort.Loader.Sony
{
  /// <summary>
  /// This XmlWriter replaces some characters with Char- or Entity- references the same way
  /// they are escaped in the original Sony XML files
  /// </summary>
  class CustomXmlWriter : XmlWriter
  {
    private static readonly char[] CharsToEscape = { '\'', '\"', '&', '<', '>' };
    private static readonly string[] CharEntites = { "apos", "quot", "amp", "lt", "gt" };

    private XmlWriter w;
    private readonly bool escapeAsEntityRef; // if true, use &amp; otherwise &#34;

    public CustomXmlWriter(TextWriter tw, XmlWriterSettings settings, bool useEntityRef)
    {
      this.w = XmlWriter.Create(tw, settings);
      this.escapeAsEntityRef = useEntityRef;
    }

    public override void WriteString(string text)
    {
      int i = 0, j;
      while ((j = text.IndexOfAny(CharsToEscape, i)) >= 0)
      {
        this.w.WriteString(text.Substring(i, j - i));
        if (this.escapeAsEntityRef)
        {
          // => &amp;
          int k = Array.IndexOf(CharsToEscape, text[j]);
          this.w.WriteEntityRef(CharEntites[k]);
        }
        else
        {
          // => &#38;
          //this.w.WriteCharEntity(text[j]);
          this.w.WriteRaw("&#" + (int)text[j] + ";");
        }

        i = j + 1;
      }
      this.w.WriteString(text.Substring(i));
    }

    #region 1:1 delegation

    public override void WriteStartDocument()
    {
      this.w.WriteStartDocument();
    }

    public override void WriteStartDocument(bool standalone)
    {
      this.w.WriteStartDocument(standalone);
    }

    public override void WriteEndDocument()
    {
      this.w.WriteEndDocument();
    }

    public override void WriteDocType(string name, string pubid, string sysid, string subset)
    {
      this.w.WriteDocType(name, pubid, sysid, subset);
    }

    public override void WriteStartElement(string prefix, string localName, string ns)
    {
      this.w.WriteStartElement(prefix, localName, ns);
    }

    public override void WriteEndElement()
    {
      this.w.WriteEndElement();
    }

    public override void WriteFullEndElement()
    {
      this.w.WriteFullEndElement();
    }

    public override void WriteStartAttribute(string prefix, string localName, string ns)
    {
      this.w.WriteStartAttribute(prefix, localName, ns);
    }

    public override void WriteEndAttribute()
    {
      this.w.WriteEndAttribute();
    }

    public override void WriteCData(string text)
    {
      this.w.WriteCData(text);
    }

    public override void WriteComment(string text)
    {
      this.w.WriteComment(text);
    }

    public override void WriteProcessingInstruction(string name, string text)
    {
      this.w.WriteProcessingInstruction(name, text);
    }

    public override void WriteEntityRef(string name)
    {
      this.w.WriteEntityRef(name);
    }

    public override void WriteCharEntity(char ch)
    {
      this.w.WriteCharEntity(ch);
    }

    public override void WriteWhitespace(string ws)
    {
      this.w.WriteWhitespace(ws);
    }

    public override void WriteSurrogateCharEntity(char lowChar, char highChar)
    {
      this.w.WriteSurrogateCharEntity(lowChar, highChar);
    }

    public override void WriteChars(char[] buffer, int index, int count)
    {
      this.w.WriteChars(buffer, index, count);
    }

    public override void WriteRaw(char[] buffer, int index, int count)
    {
      this.w.WriteRaw(buffer, index, count);
    }

    public override void WriteRaw(string data)
    {
      this.w.WriteRaw(data);
    }

    public override void WriteBase64(byte[] buffer, int index, int count)
    {
      this.w.WriteBase64(buffer, index, count);
    }

    public override void Close()
    {
      this.w.Close();
    }

    public override void Flush()
    {
      this.w.Flush();
    }

    public override string LookupPrefix(string ns)
    {
      return this.w.LookupPrefix(ns);
    }

    public override WriteState WriteState => this.w.WriteState;
    
    #endregion
  }
}