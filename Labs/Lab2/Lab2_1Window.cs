using Labs.Utility;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;

namespace Labs.Lab2
{
    class Lab2_1Window : GameWindow
    {        
        private int[] mVertexBufferObjectIDArray = new int[2];
        private int[] mSquareBufferObjectIDArray = new int[2];
        private int[] mVertexArrayObjectIDs = new int[2];
        private ShaderUtility mShader;

        public Lab2_1Window()
            : base(
                800, // Width
                600, // Height
                GraphicsMode.Default,
                "Lab 2_1 Linking to Shaders and VAOs",
                GameWindowFlags.Default,
                DisplayDevice.Default,
                3, // major
                3, // minor
                GraphicsContextFlags.ForwardCompatible
                )
        {
        }

        protected override void OnLoad(EventArgs e)
        {
            GL.ClearColor(Color4.CadetBlue);



            GL.Enable(EnableCap.DepthTest);
            //L21T5 Added depth by adding an extra vertex parameter, changing the shader variable and linking and enabling depth testing
            
            //Triangle data
            float[] tVertices = new float[] { -0.8f, 0.8f, 0.4f, 0.8f, 0.2f, 1.0f,
                                              -0.6f, -0.4f, 0.4f, 0.2f, 1.0f, 0.8f,
                                               0.2f, 0.2f, 0.4f, 1.0f, 0.8f, 0.2f };

            uint[] tIndices = new uint[] { 0, 1, 2 };

            //Square data
            float[] sVertices = new float[] { -0.2f, -0.4f, 0.2f, 1.0f, 0.5f, 0.9f,
                                               0.8f, -0.4f, 0.2f, 0.5f, 1.0f, 0.9f,
                                              -0.2f, 0.6f, 0.2f, 0.5f, 0.9f, 1.0f,
                                               0.8f, 0.6f, 0.2f, 0.0f, 0.0f, 0.0f };

            uint[] sIndices = new uint[] { 0, 1, 3, 2 };
            //L21T7 Changed vertex colours to see fragments colour values being blended together

            //Triangle buffers
            GL.GenBuffers(2, mVertexBufferObjectIDArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVertexBufferObjectIDArray[0]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(tVertices.Length * sizeof(float)), tVertices, BufferUsageHint.StaticDraw);

            int size;
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);

            if (tVertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVertexBufferObjectIDArray[1]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(tIndices.Length * sizeof(int)), tIndices, BufferUsageHint.StaticDraw);

            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);

            if (tIndices.Length * sizeof(int) != size)
            {
                throw new ApplicationException("Index data not loaded onto graphics card correctly");
            }

            //Square Buffers
            GL.GenBuffers(2, mSquareBufferObjectIDArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mSquareBufferObjectIDArray[0]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(sVertices.Length * sizeof(float)), sVertices, BufferUsageHint.StaticDraw);

            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);

            if(sVertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mSquareBufferObjectIDArray[1]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(sIndices.Length * sizeof(int)), sIndices, BufferUsageHint.StaticDraw);

            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);

            if (sIndices.Length * sizeof(int) != size)
            {
                throw new ApplicationException("Index data not loaded onto graphics card correctly");
            }

            

            #region Shader Loading Code

            mShader = new ShaderUtility(@"Lab2/Shaders/vLab21.vert", @"Lab2/Shaders/fSimple.frag");

            #endregion
            int vColourLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vColour");
            GL.EnableVertexAttribArray(vColourLocation);
            int vPositionLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vPosition");
            GL.EnableVertexAttribArray(vPositionLocation);
            //Square buffer binding
            GL.GenVertexArrays(2, mVertexArrayObjectIDs);

            GL.BindVertexArray(mVertexArrayObjectIDs[1]);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mSquareBufferObjectIDArray[0]);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mSquareBufferObjectIDArray[1]);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 *
            sizeof(float), 0);
            GL.VertexAttribPointer(vColourLocation, 3, VertexAttribPointerType.Float, false, 6 *
            sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(vColourLocation);
            GL.EnableVertexAttribArray(vPositionLocation);

            //Triangle buffer binding

            GL.BindVertexArray(mVertexArrayObjectIDs[0]);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVertexBufferObjectIDArray[0]);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVertexBufferObjectIDArray[1]);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 *
            sizeof(float), 0);
            GL.VertexAttribPointer(vColourLocation, 3, VertexAttribPointerType.Float, false, 6 *
            sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(vColourLocation);
            GL.EnableVertexAttribArray(vPositionLocation);
            
            

            base.OnLoad(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            #region Shader Loading Code

            GL.UseProgram(mShader.ShaderProgramID);

            #endregion
            //Square buffer
            GL.BindVertexArray(mVertexArrayObjectIDs[1]);
            GL.DrawElements(PrimitiveType.TriangleFan, 4, DrawElementsType.UnsignedInt, 0);

            //Triangle buffer
            GL.BindVertexArray(mVertexArrayObjectIDs[0]);
            GL.DrawElements(PrimitiveType.Triangles, 3, DrawElementsType.UnsignedInt, 0);

            GL.BindVertexArray(0);
            this.SwapBuffers();
        }

        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);
            GL.DeleteBuffers(2, mVertexBufferObjectIDArray);
            GL.DeleteBuffers(2, mSquareBufferObjectIDArray);
            GL.BindVertexArray(0);
            GL.DeleteVertexArrays(2, mVertexArrayObjectIDs);
            GL.UseProgram(0);
            mShader.Delete();
        }
    }
}
//L21T1 Rendered a big black triangle!
