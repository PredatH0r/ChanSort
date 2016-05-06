using System.CodeDom;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using DevExpress.Utils;

namespace ChanSort.Ui
{
  #region class GlobalImageCollectionHolder
  [ToolboxItem(false)]
  public class GlobalImageCollectionHolder : Component
  {
    private IContainer components;
    private SharedImageCollection sharedImageCollection1;

    public GlobalImageCollectionHolder()
    {
      InitializeComponent();
    }

    public GlobalImageCollectionHolder(IContainer container)
    {
      if (container != null)
        container.Add(this);
      InitializeComponent();
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Component Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GlobalImageCollectionHolder));
      this.sharedImageCollection1 = new DevExpress.Utils.SharedImageCollection(this.components);
      ((System.ComponentModel.ISupportInitialize)(this.sharedImageCollection1)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.sharedImageCollection1.ImageSource)).BeginInit();
      // 
      // sharedImageCollection1
      // 
      // 
      // 
      // 
      this.sharedImageCollection1.ImageSource.ImageStream = ((DevExpress.Utils.ImageCollectionStreamer)(resources.GetObject("sharedImageCollection1.ImageSource.ImageStream")));
      this.sharedImageCollection1.ImageSource.Images.SetKeyName(36, "0036.png");
      this.sharedImageCollection1.ImageSource.Images.SetKeyName(37, "0037.png");
      this.sharedImageCollection1.ImageSource.Images.SetKeyName(38, "0038.png");
      this.sharedImageCollection1.ImageSource.Images.SetKeyName(39, "0039.png");
      this.sharedImageCollection1.ImageSource.Images.SetKeyName(40, "0040.png");
      this.sharedImageCollection1.ParentControl = null;
      ((System.ComponentModel.ISupportInitialize)(this.sharedImageCollection1.ImageSource)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.sharedImageCollection1)).EndInit();

    }

    #endregion

    public Images Images { get { return sharedImageCollection1.ImageSource.Images; } }
  }
  #endregion

  #region class GlobalImageCollection
  [DesignerSerializer(typeof(GlobalImageCollectionCodeDomSerializer), typeof(CodeDomSerializer))]
  public class GlobalImageCollection : SharedImageCollection
  {
    protected static GlobalImageCollectionHolder holder = new GlobalImageCollectionHolder();
    public static Images Images { get { return holder.Images; } }

    public GlobalImageCollection() { }
    public GlobalImageCollection(IContainer container) : base(container) { }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new ImageCollection ImageSource { get { return base.ImageSource; } }
  }
  #endregion

  #region class GlobalImageCollectionCodeDomSerializer

  internal class GlobalImageCollectionCodeDomSerializer : CodeDomSerializer
  {    
    public override object Deserialize(IDesignerSerializationManager manager, object codeObject)
    {
      var baseSerializer = (CodeDomSerializer)manager.GetSerializer(typeof(SharedImageCollection), typeof(CodeDomSerializer));
      return baseSerializer.Deserialize(manager, codeObject);
    }
    
    public override object Serialize(IDesignerSerializationManager manager, object value)
    {
      var baseSerializer = (CodeDomSerializer)manager.GetSerializer(typeof(SharedImageCollection), typeof(CodeDomSerializer));
      object codeObject = baseSerializer.Serialize(manager, value);

      // remove all generated code except for the member initialization
      CodeStatementCollection coll = codeObject as CodeStatementCollection;
      if (coll != null)
      {
        for (int i=coll.Count-1; i>=0; i--)
        {
          CodeStatement ex = coll[i];
          var ass = ex as CodeAssignStatement;
          if (ass == null || !(ass.Left is CodeFieldReferenceExpression))
              coll.RemoveAt(i);
        }
      }
      return codeObject;
    }
  }
  #endregion
}
