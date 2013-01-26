using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

using Microsoft.DirectX;
using Microsoft.DirectX.DirectInput;
using Microsoft.DirectX.Direct3D;

namespace PSSGManager {
	public class ModelView : UserControl {
		public Microsoft.DirectX.Direct3D.Device device = null;
        private System.Diagnostics.Process process1;
		public Dictionary<String, RenderDataSource> renderDataSources;
		private Model model;
		private VertexBuffer vertexBuffer;
		private IndexBuffer indexBuffer;
		public float rot = 0;
        private float currentZoom = 8;

		protected bool initialised = false;
		public bool Initialised {
			get { return initialised; }
		}

     
		public void InitialiseGraphics(bool wireframe = false) {
            //Lighting in Initialise

			PresentParameters pp = new PresentParameters();
			pp.Windowed = true;
			pp.SwapEffect = SwapEffect.Discard;
			pp.EnableAutoDepthStencil = true;
			pp.AutoDepthStencilFormat = DepthFormat.D16;
			pp.DeviceWindowHandle = this.Handle;

			device = new Microsoft.DirectX.Direct3D.Device(0,Microsoft.DirectX.Direct3D.DeviceType.Hardware, this, CreateFlags.HardwareVertexProcessing, pp);
            if (wireframe)
            {
               	device.RenderState.FillMode = FillMode.WireFrame;
            }
			device.DeviceReset += new EventHandler(this.OnDeviceReset);

			OnDeviceReset(device, null);

            float x = (float)Math.Cos(rot);
            float z = (float)Math.Sin(rot);
            device.Transform.Projection = Matrix.PerspectiveFovLH((float)Math.PI / 4, this.Width / this.Height, 1f, 50f);
            device.Transform.View = Matrix.LookAtLH(new Vector3(x, currentZoom, z), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
            device.RenderState.Lighting = true;
            device.Lights[0].Type = LightType.Directional;
            device.Lights[0].Diffuse = Color.White;
            device.Lights[0].Direction = new Vector3(-x, -6, -z);
            device.Lights[0].Position = new Vector3(x, 6, z);
            device.Lights[0].Enabled = true;
			initialised = true;
		}

		public void OnDeviceReset(object sender, EventArgs e) {
			device = sender as Microsoft.DirectX.Direct3D.Device;
		}

		private void Render() {
			device.BeginScene();

            float x = (float)Math.Cos(rot);
	     	float z = (float)Math.Sin(rot);

        device.Transform.Projection = Matrix.PerspectiveFovLH((float)Math.PI / 4, this.Width / this.Height, 1f, 50f);

                                     
     device.Transform.View = Matrix.LookAtLH(new Vector3(x,currentZoom, z), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
            device.Transform.World = model.transform;
			device.VertexFormat = CustomVertex.PositionNormalColored.Format;
			device.SetStreamSource(0, vertexBuffer, 0);
			device.Indices = indexBuffer;

			device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, model.getVertices().Length, 0, model.getIndices().Length / 3);

			device.EndScene();
		}

		public void RenderModel(Model model) {
			this.model = model;
			vertexBuffer = new VertexBuffer(typeof(CustomVertex.PositionNormalColored), model.getVertices().Length,
										device, Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionNormalColored.Format, Pool.Default);
			vertexBuffer.SetData(model.getVertices(), 0, LockFlags.None);

			indexBuffer = new IndexBuffer(typeof(ushort), model.getIndices().Length, device, Usage.WriteOnly, Pool.Default);
			indexBuffer.SetData(model.getIndices(), 0, LockFlags.None);

			Render();
		}

	

		protected override void OnPaint(PaintEventArgs e) {
			if (device == null || model == null) {
				e.Graphics.FillRectangle(Brushes.Black, new Rectangle(0, 0, Width, Height));
				return;
			}


         
    

		//	Render();
		}

		protected override void OnPaintBackground(PaintEventArgs e) {
		}

		protected override void OnSizeChanged(EventArgs e) {
			Invalidate();
            InitialiseGraphics();

		}

		private void InitializeComponent() {
            this.process1 = new System.Diagnostics.Process();
            this.SuspendLayout();
            // 
            // process1
            // 
            this.process1.StartInfo.Domain = "";
            this.process1.StartInfo.LoadUserProfile = false;
            this.process1.StartInfo.Password = null;
            this.process1.StartInfo.StandardErrorEncoding = null;
            this.process1.StartInfo.StandardOutputEncoding = null;
            this.process1.StartInfo.UserName = "";
            this.process1.SynchronizingObject = this;
            // 
            // ModelView
            // 
            this.Name = "ModelView";
            this.Size = new System.Drawing.Size(458, 302);
            this.Load += new System.EventHandler(this.ModelView_Load);
            this.ResumeLayout(false);



		}

        private void ModelView_Load(object sender, EventArgs e)
        {

        }
	}

	public class Model {
		public string name;
        public string lod;
		public RenderDataSource renderDataSource;

		public Matrix transform;

		public int streamOffset;
		public int elementCount;
		public int indexOffset;
		public int indicesCount;

		public string blah;

		public Model(string name, string lod, RenderDataSource renderDataSource, Matrix transform, int streamOffset, int elementCount, int indexOffset, int indicesCount) {
			this.name = name;
            this.lod = lod;
			this.renderDataSource = renderDataSource;
			this.transform = transform;
			this.streamOffset = streamOffset; // Unsure what these are for, vertex stream should not be split up!
			this.elementCount = elementCount; //
			this.indexOffset = indexOffset;
			this.indicesCount = indicesCount;
		}

		public CustomVertex.PositionNormalColored[] getVertices() {
			return renderDataSource.vertices;
		}

		public ushort[] getIndices() {
			ushort[] indices = new ushort[indicesCount];
			Array.Copy(renderDataSource.indices, indexOffset, indices, 0, indicesCount);
			return indices;
		}

		public override string ToString() {
			return lod + name;
		}
	}

	public class RenderDataSource {
		public string name;
		public CustomVertex.PositionNormalColored[] vertices;
		public ushort[] indices;

		public RenderDataSource(string name, CustomVertex.PositionNormalColored[] vertices, ushort[] indices) {
			this.name = name;
			this.vertices = vertices;
			this.indices = indices;
		}

		public override string ToString() {
			return name;
		}
	}
}

