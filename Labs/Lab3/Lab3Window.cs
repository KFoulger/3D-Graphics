using Labs.Utility;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;

namespace Labs.Lab3
{
    public class Lab3Window : GameWindow
    {
        public Lab3Window()
            : base(
                800, // Width
                600, // Height
                GraphicsMode.Default,
                "Lab 3 Lighting and Material Properties",
                GameWindowFlags.Default,
                DisplayDevice.Default,
                3, // major
                3, // minor
                GraphicsContextFlags.ForwardCompatible
                )
        {
        }

        private int[] mVBO_IDs = new int[5];
        private int[] mVAO_IDs = new int[3];
        private ShaderUtility mShader;
        private ModelUtility mArmadilloManModelUtility, mCylinderModelUtility;
        private Matrix4 mView, mArmadilloManModel, mCylinderModel, mGroundModel;

        protected override void OnLoad(EventArgs e)
        {
            // Set some GL state
            GL.ClearColor(Color4.White);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);

            mShader = new ShaderUtility(@"Lab3/Shaders/vPassThrough.vert", @"Lab3/Shaders/fLighting.frag");
            GL.UseProgram(mShader.ShaderProgramID);
            int vPositionLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vPosition");
            int vNormalLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vNormal");

            LightPosition();

            GL.GenVertexArrays(mVAO_IDs.Length, mVAO_IDs);
            GL.GenBuffers(mVBO_IDs.Length, mVBO_IDs);

            float[] vertices = new float[] {-10, 0, -10,0,1,0,
                                             -10, 0, 10,0,1,0,
                                             10, 0, 10,0,1,0,
                                             10, 0, -10,0,1,0,};

            GL.BindVertexArray(mVAO_IDs[0]);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_IDs[0]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * sizeof(float)), vertices, BufferUsageHint.StaticDraw);

            int size;
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);

            GL.EnableVertexAttribArray(vNormalLocation);
            GL.VertexAttribPointer(vNormalLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 12);

            mArmadilloManModelUtility = ModelUtility.LoadModel(@"Utility/Models/model.bin");

            GL.BindVertexArray(mVAO_IDs[1]);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_IDs[1]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(mArmadilloManModelUtility.Vertices.Length * sizeof(float)), mArmadilloManModelUtility.Vertices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVBO_IDs[2]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(mArmadilloManModelUtility.Indices.Length * sizeof(float)), mArmadilloManModelUtility.Indices, BufferUsageHint.StaticDraw);

            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (mArmadilloManModelUtility.Vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
            if (mArmadilloManModelUtility.Indices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Index data not loaded onto graphics card correctly");
            }

            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);

            GL.EnableVertexAttribArray(vNormalLocation);
            GL.VertexAttribPointer(vNormalLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 12);

            //////////////////////////////////////////////////////////////////////////////////

            mCylinderModelUtility = ModelUtility.LoadModel(@"Utility/Models/cylinder.bin");

            GL.BindVertexArray(mVAO_IDs[2]);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_IDs[3]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(mCylinderModelUtility.Vertices.Length * sizeof(float)), mCylinderModelUtility.Vertices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVBO_IDs[4]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(mCylinderModelUtility.Indices.Length * sizeof(float)), mCylinderModelUtility.Indices, BufferUsageHint.StaticDraw);

            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (mCylinderModelUtility.Vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
            if (mCylinderModelUtility.Indices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Index data not loaded onto graphics card correctly");
            }

            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);

            GL.EnableVertexAttribArray(vNormalLocation);
            GL.VertexAttribPointer(vNormalLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 12);

            GL.BindVertexArray(0);

            mView = Matrix4.CreateTranslation(0, -1.5f, 0);
            int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
            GL.UniformMatrix4(uView, true, ref mView);

            mGroundModel = Matrix4.CreateTranslation(0, 0, -5f);
            mArmadilloManModel = Matrix4.CreateTranslation(0, 3, -5f);
            mCylinderModel = Matrix4.CreateTranslation(0, 1, -5f);

            LightPosition();
            MaterialSetter();

            int uEyePositionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uEyePosition");
            Vector4 lightPosition = Vector4.Transform(new Vector4(2, 1, -8.5f, 1), mView);
            GL.Uniform4(uEyePositionLocation, lightPosition);
            base.OnLoad(e);

        }

        private void LightPosition()
        {
            int uLightPositionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[0].Position");
            Vector4 lightPosition = new Vector4(2, 4, -8.5f, 1);
            lightPosition = Vector4.Transform(lightPosition, mView);
            GL.Uniform4(uLightPositionLocation, lightPosition);

            int uAmbientLightLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[0].AmbientLight");
            Vector3 colour = new Vector3(1.0f, 0.0f, 0.0f);
            GL.Uniform3(uAmbientLightLocation, colour);

            int uDiffuseLightLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[0].DiffuseLight");
            colour = new Vector3(1.0f, 0.0f, 0.0f);
            GL.Uniform3(uDiffuseLightLocation, colour);

            int uSpecularLightLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[0].SpecularLight");
            colour = new Vector3(1.0f, 0.0f, 0.0f);
            GL.Uniform3(uSpecularLightLocation, colour);

            /////////////////////////////////////////////////////////////////////

            uLightPositionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[1].Position");
            lightPosition = new Vector4(-2, 5, -6.5f, 1);
            lightPosition = Vector4.Transform(lightPosition, mView);
            GL.Uniform4(uLightPositionLocation, lightPosition);

            uAmbientLightLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[1].AmbientLight");
            colour = new Vector3(0.2f, 0.0f, 1.0f);
            GL.Uniform3(uAmbientLightLocation, colour);

            uDiffuseLightLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[1].DiffuseLight");
            colour = new Vector3(0.2f, 0.0f, 1.0f);
            GL.Uniform3(uDiffuseLightLocation, colour);

            uSpecularLightLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[1].SpecularLight");
            colour = new Vector3(0.2f, 0.0f, 1.0f);
            GL.Uniform3(uSpecularLightLocation, colour);

            //////////////////////////////////////////////////////////////////////

            uLightPositionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[2].Position");
            lightPosition = new Vector4(8, 6, -9.5f, 1);
            lightPosition = Vector4.Transform(lightPosition, mView);
            GL.Uniform4(uLightPositionLocation, lightPosition);

            uAmbientLightLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[2].AmbientLight");
            colour = new Vector3(0.0f, 1.0f, 0.0f);
            GL.Uniform3(uAmbientLightLocation, colour);

            uDiffuseLightLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[2].DiffuseLight");
            colour = new Vector3(0.0f, 1.0f, 0.0f);
            GL.Uniform3(uDiffuseLightLocation, colour);

            uSpecularLightLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[2].SpecularLight");
            colour = new Vector3(0.0f, 1.0f, 0.0f);
            GL.Uniform3(uSpecularLightLocation, colour);

        }

        private void MaterialSetter()
        {
            int uAmbientReflectivityLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uMaterial.AmbientReflectivity");
            Vector3 uAmbientReflectivity = new Vector3(0.5f, 0.5f, 0.5f);
            GL.Uniform3(uAmbientReflectivityLocation, uAmbientReflectivity);

            int uDiffuseReflectivityLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uMaterial.DiffuseReflectivity");
            Vector3 uDiffuseReflectivity = new Vector3(0.5f, 0.5f, 0.5f);
            GL.Uniform3(uDiffuseReflectivityLocation, uDiffuseReflectivity);

            int uSpecularReflectivityLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uMaterial.SpecularReflectivity");
            Vector3 uSpecularReflectivity = new Vector3(0.7f, 0.7f, 0.7f);
            GL.Uniform3(uSpecularReflectivityLocation, uSpecularReflectivity);

            int uShininessLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uMaterial.Shininess");
            float uShininess = 0.078125f;
            GL.Uniform1(uShininessLocation, uShininess);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(this.ClientRectangle);
            if (mShader != null)
            {
                int uProjectionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uProjection");
                Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(1, (float)ClientRectangle.Width / ClientRectangle.Height, 0.5f, 25);
                GL.UniformMatrix4(uProjectionLocation, true, ref projection);
            }
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            if (e.KeyChar == 'w')
            {
 
                mView = mView * Matrix4.CreateTranslation(0.0f, 0.0f, 0.05f);
                int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
                int uEyePositionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uEyePosition");
                Vector4 lightPosition = Vector4.Transform(new Vector4(2, 1, -8.5f, 1), mView);
                GL.Uniform4(uEyePositionLocation, lightPosition);
                GL.UniformMatrix4(uView, true, ref mView);
                LightPosition();
            }
            if(e.KeyChar == 'a')
            {
                mView = mView * Matrix4.CreateRotationY(-0.025f);
                int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
                int uEyePositionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uEyePosition");
                Vector4 lightPosition = Vector4.Transform(new Vector4(2, 1, -8.5f, 1), mView);
                GL.Uniform4(uEyePositionLocation, lightPosition);
                GL.UniformMatrix4(uView, true, ref mView);
                LightPosition();
            }
            if (e.KeyChar == 'd')
            {
                mView = mView * Matrix4.CreateRotationY(0.025f);
                int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
                int uEyePositionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uEyePosition");
                Vector4 lightPosition = Vector4.Transform(new Vector4(2, 1, -8.5f, 1), mView);
                GL.Uniform4(uEyePositionLocation, lightPosition);
                GL.UniformMatrix4(uView, true, ref mView);
                LightPosition();
            }
            if (e.KeyChar == 'z')
            {

                Vector3 t = mGroundModel.ExtractTranslation();
                Matrix4 translation = Matrix4.CreateTranslation(t);
                Matrix4 inverseTranslation = Matrix4.CreateTranslation(-t);
                mGroundModel = mGroundModel * inverseTranslation * Matrix4.CreateRotationY(-0.025f) *
                translation;
                LightPosition();
            }
            if(e.KeyChar == 's')
            {

                mView = mView * Matrix4.CreateTranslation(0.0f, 0.0f, -0.05f);
                int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
                GL.UniformMatrix4(uView, true, ref mView);
                int uEyePositionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uEyePosition");
                Vector4 lightPosition = Vector4.Transform(new Vector4(2, 1, -8.5f, 1), mView);
                GL.Uniform4(uEyePositionLocation, lightPosition);
                LightPosition();
            }
            if(e.KeyChar == 'x')
            {

                Vector3 t = mGroundModel.ExtractTranslation();
                Matrix4 translation = Matrix4.CreateTranslation(t);
                Matrix4 inverseTranslation = Matrix4.CreateTranslation(-t);
                mGroundModel = mGroundModel * inverseTranslation * Matrix4.CreateRotationY(0.025f) *
                translation;
                LightPosition();
            }
            if(e.KeyChar == 'c')
            {
                Vector3 t = mArmadilloManModel.ExtractTranslation();
                Matrix4 translation = Matrix4.CreateTranslation(t);
                Matrix4 inverseTranslation = Matrix4.CreateTranslation(-t);
                mArmadilloManModel = mArmadilloManModel * inverseTranslation * Matrix4.CreateRotationY(0.025f) *
                translation;
            }
            if(e.KeyChar == 'v')
            {
                Vector3 t = mArmadilloManModel.ExtractTranslation();
                Matrix4 translation = Matrix4.CreateTranslation(t);
                Matrix4 inverseTranslation = Matrix4.CreateTranslation(-t);
                mArmadilloManModel = mArmadilloManModel * inverseTranslation * Matrix4.CreateRotationY(-0.025f) *
                translation;
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            LightPosition();
            int uModel = GL.GetUniformLocation(mShader.ShaderProgramID, "uModel");
            GL.UniformMatrix4(uModel, true, ref mGroundModel);  

            GL.BindVertexArray(mVAO_IDs[0]);
            GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4);

            Matrix4 m = mArmadilloManModel * mGroundModel;
            uModel = GL.GetUniformLocation(mShader.ShaderProgramID, "uModel");
            GL.UniformMatrix4(uModel, true, ref m); 

            GL.BindVertexArray(mVAO_IDs[1]);
            GL.DrawElements(PrimitiveType.Triangles, mArmadilloManModelUtility.Indices.Length, DrawElementsType.UnsignedInt, 0);

            ////////////////////////////////////////////////////////////////////

            m = mCylinderModel * mGroundModel;
            uModel = GL.GetUniformLocation(mShader.ShaderProgramID, "uModel");
            GL.UniformMatrix4(uModel, true, ref m);

            GL.BindVertexArray(mVAO_IDs[2]);
            GL.DrawElements(PrimitiveType.Triangles, mCylinderModelUtility.Indices.Length, DrawElementsType.UnsignedInt, 0);

            GL.BindVertexArray(0);
            this.SwapBuffers();
        }

        protected override void OnUnload(EventArgs e)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.DeleteBuffers(mVBO_IDs.Length, mVBO_IDs);
            GL.DeleteVertexArrays(mVAO_IDs.Length, mVAO_IDs);
            mShader.Delete();
            base.OnUnload(e);
        }
    }
}
