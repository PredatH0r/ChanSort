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
      // 
      // sharedImageCollection1
      // 
      // 
      // 
      // 
      this.sharedImageCollection1.ImageSource.ImageStream = ((DevExpress.Utils.ImageCollectionStreamer)(resources.GetObject("sharedImageCollection1.ImageSource.ImageStream")));
      this.sharedImageCollection1.ImageSource.Images.SetKeyName(0, "0000.png");
      this.sharedImageCollection1.ImageSource.Images.SetKeyName(1, "0001.png");
      this.sharedImageCollection1.ImageSource.Images.SetKeyName(2, "0002.png");
      this.sharedImageCollection1.ImageSource.Images.SetKeyName(3, "0003.png");
      this.sharedImageCollection1.ImageSource.Images.SetKeyName(4, "0004.png");
      this.sharedImageCollection1.ImageSource.Images.SetKeyName(5, "0005.png");
      this.sharedImageCollection1.ImageSource.Images.SetKeyName(6, "0006.png");
      this.sharedImageCollection1.ImageSource.Images.SetKeyName(7, "0007.png");
      this.sharedImageCollection1.ImageSource.Images.SetKeyName(8, "0008.png");
      this.sharedImageCollection1.ImageSource.Images.SetKeyName(9, "0009.png");
      this.sharedImageCollection1.ImageSource.Images.SetKeyName(10, "0010.png");
      this.sharedImageCollection1.ImageSource.Images.SetKeyName(11, "0011.png");
      this.sharedImageCollection1.ImageSource.Images.SetKeyName(12, "0012.png");
      this.sharedImageCollection1.ImageSource.Images.SetKeyName(13, "0013.png");
      this.sharedImageCollection1.ImageSource.Images.SetKeyName(14, "0014.png");
      this.sharedImageCollection1.ImageSource.Images.SetKeyName(15, "0015.png");
      this.sharedImageCollection1.ImageSource.Images.SetKeyName(16, "0016.png");
      this.sharedImageCollection1.ImageSource.Images.SetKeyName(17, "0017.png");
      this.sharedImageCollection1.ImageSource.Images.SetKeyName(18, "0018.png");
      this.sharedImageCollection1.ImageSource.Images.SetKeyName(19, "0019.png");
      this.sharedImageCollection1.ImageSource.Images.SetKeyName(20, "0020.png");
      this.sharedImageCollection1.ImageSource.Images.SetKeyName(21, "0021.png");
      this.sharedImageCollection1.ImageSource.Images.SetKeyName(22, "0022.png");
      this.sharedImageCollection1.ImageSource.Images.SetKeyName(23, "0023.png");
      this.sharedImageCollection1.ImageSource.Images.SetKeyName(24, "0024.png");
      this.sharedImageCollection1.ImageSource.Images.SetKeyName(25, "0025.png");
      this.sharedImageCollection1.ImageSource.Images.SetKeyName(26, "0026.png");
      this.sharedImageCollection1.ImageSource.Images.SetKeyName(27, "0027.png");
      this.sharedImageCollection1.ImageSource.Images.SetKeyName(28, "0028.png");
      this.sharedImageCollection1.ImageSource.Images.SetKeyName(29, "0029.png");
      this.sharedImageCollection1.ImageSource.Images.SetKeyName(30, "0030.png");
      this.sharedImageCollection1.ImageSource.Images.SetKeyName(31, "0031.png");
      this.sharedImageCollection1.ImageSource.Images.SetKeyName(32, "0032.png");
      this.sharedImageCollection1.ImageSource.Images.SetKeyName(33, "0033.png");
      this.sharedImageCollection1.ParentControl = null;
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
