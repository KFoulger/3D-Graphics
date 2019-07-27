using Labs.Utility;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Drawing;
using System.Drawing.Imaging;


namespace Labs.ACW
{
    public class ACWWindow : GameWindow
    {
        public ACWWindow()
            : base(
                800, // Width
                600, // Height
                GraphicsMode.Default,
                "Assessed Coursework",
                GameWindowFlags.Default,
                DisplayDevice.Default,
                3, // major
                3, // minor
                GraphicsContextFlags.ForwardCompatible
                )
        {
        }

        private int[] mVBO_IDs = new int[7];
        private int[] mVAO_IDs = new int[4];
        private int[] mTexture_IDs = new int[4];
        private ShaderUtility mShader;
        private ModelUtility mArmadilloManModelUtility, mCylinderModelUtility, mBallModelUtility;
        private Matrix4 mView, mArmadilloManModel, mCylinderModel, mBallModel, mGroundModel;
        private float movementRate = 0.1f;
        private float startingPosition = 0.0f;
        private float rotation = -1.7f;
        private float rotationRate = 0.00f;
        private float ballRotation = 0.0f;
        private float ballRR = 0.05f;
        private int locked = 1;

        protected override void OnLoad(EventArgs e)
        {
            // Set some GL state
            GL.ClearColor(Color4.LightSkyBlue);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            
            mShader = new ShaderUtility(@"ACW/Shaders/vPassThrough.vert", @"ACW/Shaders/fLighting.frag");//Sets the shaders for the program

            GL.UseProgram(mShader.ShaderProgramID);
            
            int vPositionLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vPosition");
            int vNormalLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vNormal");
            int vTexCoordsLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vTexCoords");
            //(54-56)Saves the location of shader locations as variables
            GL.GenVertexArrays(mVAO_IDs.Length, mVAO_IDs);
            GL.GenBuffers(mVBO_IDs.Length, mVBO_IDs);

            float[] vertices = new float[] {-10, 0, -10, 0, 1, 0, 0.0f, 0.0f,
                                             -10, 0, 10, 0, 1, 0, 0.0f, 1.0f,
                                             10, 0, 10, 0, 1, 0, 1.0f, 1.0f,
                                             10, 0, -10, 0, 1, 0, 1.0f, 0.0f};
            //(61-64)Sets up the ground vertices and texture coordinates
            GL.BindVertexArray(mVAO_IDs[0]);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_IDs[0]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * sizeof(float)), vertices, BufferUsageHint.StaticDraw);

            int size;
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }
            //(71-75)Binds the ground vertex array and saves it on the graphics card
            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);

            GL.EnableVertexAttribArray(vNormalLocation);
            GL.VertexAttribPointer(vNormalLocation, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 12);

            GL.EnableVertexAttribArray(vTexCoordsLocation);
            GL.VertexAttribPointer(vTexCoordsLocation, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 24);
            //(77-84)Gets the location of the position coordinates, normal coordinates and texture coordinates
            string filepath = @"ACW/texture.jpg";
            if (System.IO.File.Exists(filepath))
            {
                Bitmap TextureBitmap = new Bitmap(filepath);
                BitmapData TextureData = TextureBitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, TextureBitmap.Width,
                TextureBitmap.Height), ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppRgb);

                GL.ActiveTexture(TextureUnit.Texture0);
                GL.GenTextures(1, out mTexture_IDs[0]);
                GL.BindTexture(TextureTarget.Texture2D, mTexture_IDs[0]);
                GL.TexImage2D(TextureTarget.Texture2D,
                0, PixelInternalFormat.Rgba, TextureData.Width, TextureData.Height,
                0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                PixelType.UnsignedByte, TextureData.Scan0);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                (int)TextureMagFilter.Linear);
                TextureBitmap.UnlockBits(TextureData);

                int uTextureSamplerLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uTextureSampler[0]");
                GL.Uniform1(uTextureSamplerLocation, 0);
            }
            else
            {
                throw new Exception("Could not find file " + filepath);
            }
           //(86-114)Loads the ground texture onto the graphics card and saves its location to the texture sampler array
            mArmadilloManModelUtility = ModelUtility.LoadModel(@"Utility/Models/model.bin");
            BufferSetup(vPositionLocation, vNormalLocation, vTexCoordsLocation, mVAO_IDs[1], mVBO_IDs[1], mVBO_IDs[2], 6, mArmadilloManModelUtility);

            filepath = @"ACW/space.jpg";
            if (System.IO.File.Exists(filepath))
            {
                Bitmap TextureBitmap = new Bitmap(filepath);
                BitmapData TextureData = TextureBitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, TextureBitmap.Width,
                TextureBitmap.Height), ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppRgb);

                GL.ActiveTexture(TextureUnit.Texture1);
                GL.GenTextures(1, out mTexture_IDs[1]);
                GL.BindTexture(TextureTarget.Texture2D, mTexture_IDs[1]);
                GL.TexImage2D(TextureTarget.Texture2D,
                0, PixelInternalFormat.Rgba, TextureData.Width, TextureData.Height,
                0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                PixelType.UnsignedByte, TextureData.Scan0);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                (int)TextureMagFilter.Linear);
                TextureBitmap.UnlockBits(TextureData);

                int uTextureSamplerLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uTextureSampler[1]");
                GL.Uniform1(uTextureSamplerLocation, 1);
            }
            else
            {
                throw new Exception("Could not find file " + filepath);
            }
            //(119-147)Loads the armadillo man model texture onto the graphics card and saves its location to the texture sampler array
            mCylinderModelUtility = ModelUtility.LoadModel(@"Utility/Models/cylinder.bin");
            BufferSetup(vPositionLocation, vNormalLocation, vTexCoordsLocation, mVAO_IDs[2], mVBO_IDs[3], mVBO_IDs[4], 6, mCylinderModelUtility);

            filepath = @"ACW/metaltexture.jpg";
            if (System.IO.File.Exists(filepath))
            {
                Bitmap TextureBitmap = new Bitmap(filepath);
                BitmapData TextureData = TextureBitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, TextureBitmap.Width,
                TextureBitmap.Height), ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppRgb);

                GL.ActiveTexture(TextureUnit.Texture2);
                GL.GenTextures(1, out mTexture_IDs[2]);
                GL.BindTexture(TextureTarget.Texture2D, mTexture_IDs[2]);
                GL.TexImage2D(TextureTarget.Texture2D,
                0, PixelInternalFormat.Rgba, TextureData.Width, TextureData.Height,
                0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                PixelType.UnsignedByte, TextureData.Scan0);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                (int)TextureMagFilter.Linear);
                TextureBitmap.UnlockBits(TextureData);

                int uTextureSamplerLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uTextureSampler[2]");
                GL.Uniform1(uTextureSamplerLocation, 2);
            }
            else
            {
                throw new Exception("Could not find file " + filepath);
            }
            //(152-180)Loads the cylinder texture onto the graphics card and saves its location to the texture sampler array
            mBallModelUtility = ModelUtility.LoadModel(@"Utility/Models/sphere.bin");
            BufferSetup(vPositionLocation, vNormalLocation, vTexCoordsLocation, mVAO_IDs[3], mVBO_IDs[5], mVBO_IDs[6], 2, mBallModelUtility);

            filepath = @"ACW/texture2.jpg";
            if (System.IO.File.Exists(filepath))
            {
                Bitmap TextureBitmap = new Bitmap(filepath);
                BitmapData TextureData = TextureBitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, TextureBitmap.Width,
                TextureBitmap.Height), ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppRgb);

                GL.ActiveTexture(TextureUnit.Texture3);
                GL.GenTextures(1, out mTexture_IDs[3]);
                GL.BindTexture(TextureTarget.Texture2D, mTexture_IDs[3]);
                GL.TexImage2D(TextureTarget.Texture2D,
                0, PixelInternalFormat.Rgba, TextureData.Width, TextureData.Height,
                0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                PixelType.UnsignedByte, TextureData.Scan0);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                (int)TextureMagFilter.Linear);
                TextureBitmap.UnlockBits(TextureData);

                int uTextureSamplerLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uTextureSampler[3]");
                GL.Uniform1(uTextureSamplerLocation, 3);
            }
            else
            {
                throw new Exception("Could not find file " + filepath);
            }
            //(182-213)Loads the ball texture onto the graphics card and saves its location to the texture sampler array
            GL.BindVertexArray(0);

            mView = Matrix4.CreateTranslation(0, -3.0f, 0);
            int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
            GL.UniformMatrix4(uView, true, ref mView);
            //(217-219)Sets the camera location
            mGroundModel = Matrix4.CreateTranslation(0, 0, -5f);
            mArmadilloManModel = Matrix4.CreateRotationY(-1.7f) * Matrix4.CreateTranslation(-0.1f, 2.8f, -5f);
            mCylinderModel = Matrix4.CreateTranslation(3, 1, 0f);
            mBallModel = Matrix4.CreateTranslation(0, 1, -5f);
            //(221-224)Sets the location of the models and the ground
            LightPosition();
            MaterialSetter();

            int uEyePositionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uEyePosition");
            Vector4 lightPosition = Vector4.Transform(new Vector4(2, 1, -8.5f, 1), mView);
            GL.Uniform4(uEyePositionLocation, lightPosition);
            //(229-231)Updates the eye position for specular lighting calculations
            base.OnLoad(e);
        }
        /// <summary>
        /// Sets up the buffers for models
        /// </summary>
        /// <param name="vPositionLocation"></param>
        /// <param name="vNormalLocation"></param>
        /// <param name="vTexCoordsLocation"></param>
        /// <param name="VAO"> The vertex array of the model </param>
        /// <param name="VBO1"> The first vertex buffer </param>
        /// <param name="VBO2"> The second vertex buffer </param>
        /// <param name="textureNo"> The number of coordinates for the texture </param>
        /// <param name="model"> The model being set up </param>
        private void BufferSetup(int vPositionLocation, int vNormalLocation, int vTexCoordsLocation, int VAO, int VBO1, int VBO2, int textureNo, ModelUtility model)
        {
            int size;
            GL.BindVertexArray(VAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO1);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(model.Vertices.Length * sizeof(float)), model.Vertices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, VBO2);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(model.Indices.Length * sizeof(float)), model.Indices, BufferUsageHint.StaticDraw);
            //(249-253)Binds the vertex buffers
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (model.Vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
            if (model.Indices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Index data not loaded onto graphics card correctly");
            }

            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);

            GL.EnableVertexAttribArray(vNormalLocation);
            GL.VertexAttribPointer(vNormalLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 12);

            GL.EnableVertexAttribArray(vTexCoordsLocation);
            GL.VertexAttribPointer(vTexCoordsLocation, 2, VertexAttribPointerType.Float, false, textureNo * sizeof(float), 24);
            //(267-274)Gets the location of the position coordinates, normal coordinates and texture coordinates
        }
        /// <summary>
        /// Updates the lights position and sets colours for the different lighting
        /// </summary>
        private void LightPosition()
        {
            ///////////////////////////// Light 1 ////////////////////////////////////////
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
            //(281-296)Sets up the position and colour of the various types of light and sends it to the shader

            ///////////////////////////// Light 2 ////////////////////////////////////////

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

            ///////////////////////////// Light 3 ////////////////////////////////////////

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
        /// <summary>
        /// Sets the material of the models
        /// </summary>
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
        /// <summary>
        /// Updates the camera and model positions based on key presses
        /// </summary>
        /// <param name="e"></param>
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            if (e.KeyChar == 'w' && locked == 1)
            {
                mView = mView * Matrix4.CreateTranslation(0.0f, 0.0f, 0.05f);
                int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
                int uEyePositionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uEyePosition");
                Vector4 lightPosition = Vector4.Transform(new Vector4(2, 1, -8.5f, 1), mView);
                GL.Uniform4(uEyePositionLocation, lightPosition);
                GL.UniformMatrix4(uView, true, ref mView);
                LightPosition();
                //(368-376)Moves the camera forward
            }
            if (e.KeyChar == 'a' && locked == 1)
            {
                mView = mView * Matrix4.CreateRotationY(-0.025f);
                int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
                int uEyePositionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uEyePosition");
                Vector4 lightPosition = Vector4.Transform(new Vector4(2, 1, -8.5f, 1), mView);
                GL.Uniform4(uEyePositionLocation, lightPosition);
                GL.UniformMatrix4(uView, true, ref mView);
                LightPosition();
                //(379-387)Moves the camera left
            }
            if (e.KeyChar == 's' && locked == 1)
            {
                mView = mView * Matrix4.CreateTranslation(0.0f, 0.0f, -0.05f);
                int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
                GL.UniformMatrix4(uView, true, ref mView);
                int uEyePositionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uEyePosition");
                Vector4 lightPosition = Vector4.Transform(new Vector4(2, 1, -8.5f, 1), mView);
                GL.Uniform4(uEyePositionLocation, lightPosition);
                LightPosition();
                //(390-398)Moves the camera right
            }
            if (e.KeyChar == 'd' && locked == 1)
            {
                mView = mView * Matrix4.CreateRotationY(0.025f);
                int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
                int uEyePositionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uEyePosition");
                Vector4 lightPosition = Vector4.Transform(new Vector4(2, 1, -8.5f, 1), mView);
                GL.Uniform4(uEyePositionLocation, lightPosition);
                GL.UniformMatrix4(uView, true, ref mView);
                LightPosition();
                //(401-409)Moves the camera backward
            }
            if (e.KeyChar == 'x')
            {
                Vector3 t = mGroundModel.ExtractTranslation();
                Matrix4 translation = Matrix4.CreateTranslation(t);
                Matrix4 inverseTranslation = Matrix4.CreateTranslation(-t);
                mGroundModel = mGroundModel * inverseTranslation * Matrix4.CreateRotationY(-0.025f) *
                translation;
                LightPosition();
                //(412-419)Rotates the ground right
            }
            if (e.KeyChar == 'z')
            {
                Vector3 t = mGroundModel.ExtractTranslation();
                Matrix4 translation = Matrix4.CreateTranslation(t);
                Matrix4 inverseTranslation = Matrix4.CreateTranslation(-t);
                mGroundModel = mGroundModel * inverseTranslation * Matrix4.CreateRotationY(0.025f) *
                translation;
                LightPosition();
                //(422-429)Rotates the ground left
            }
            if (e.KeyChar == 'v')
            {
                Vector3 t = mCylinderModel.ExtractTranslation();
                Matrix4 translation = Matrix4.CreateTranslation(t);
                Matrix4 inverseTranslation = Matrix4.CreateTranslation(-t);
                mCylinderModel = mCylinderModel * inverseTranslation * Matrix4.CreateRotationY(0.025f) *
                translation;
                //(433-438)Rotates the cylinder right
            }
            if (e.KeyChar == 'c')
            {
                Vector3 t = mCylinderModel.ExtractTranslation();
                Matrix4 translation = Matrix4.CreateTranslation(t);
                Matrix4 inverseTranslation = Matrix4.CreateTranslation(-t);
                mCylinderModel = mCylinderModel * inverseTranslation * Matrix4.CreateRotationY(-0.025f) *
                translation;
                //(441-447)Rotates the cylinder left
            }
            if (e.KeyChar == 'f')
            {
                locked = -locked;
                mView = Matrix4.CreateRotationY(-0.25f) * Matrix4.CreateRotationX(0.5f) * Matrix4.CreateTranslation(-5.0f, -6.0f, -10.0f);
                int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
                GL.UniformMatrix4(uView, true, ref mView);
                LightPosition();
                //(450-456)Locks the camera to a position
            }
        }
        /// <summary>
        /// Changes the size of the models based on window size
        /// </summary>
        /// <param name="e"></param>
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
        /// <summary>
        /// Changes the scene
        /// </summary>
        /// <param name="e"></param>
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
 	        base.OnUpdateFrame(e);
            mArmadilloManModel = Matrix4.CreateRotationY(rotation) * Matrix4.CreateTranslation(0, 2.8f, startingPosition + movementRate - 5.0f);
            mBallModel = Matrix4.CreateRotationX(ballRotation + ballRR) * Matrix4.CreateTranslation(0, 1.0f, startingPosition + movementRate - 5.0f);
            startingPosition += movementRate;
            ballRotation += ballRR;
            //(482-485)Updates the location of the armadillo man and ball
            if(startingPosition > 7 || startingPosition < -5)
            {
                movementRate = -movementRate;
                if(startingPosition > 7)
                {
                    movementRate = 0;
                    rotationRate = 0.05f;
                    rotation += rotationRate;
                    ballRR = 0;
                    if(rotation > 1.8f)
                    {
                        rotationRate = 0;
                        movementRate = -0.1f;
                        ballRR = -0.05f;
                    }
                }
                if(startingPosition < -5)
                {
                    movementRate = 0;
                    rotationRate = -0.05f;
                    rotation += rotationRate;
                    ballRR = 0;
                    if (rotation < -1.8f)
                    {
                        rotationRate = 0;
                        movementRate = 0.1f;
                        ballRR = 0.05f;
                    }
                }
            }
            //(487-516)Rotates the model to face the other way and switches the movement
        }
        /// <summary>
        /// Sets up the frame with the models in it
        /// </summary>
        /// <param name="e"></param>
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            int uTextureSelectLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "textureSelect");

            int uModel = GL.GetUniformLocation(mShader.ShaderProgramID, "uModel");
            GL.UniformMatrix4(uModel, true, ref mGroundModel);

            GL.Uniform1(uTextureSelectLocation, 0);
            GL.BindVertexArray(mVAO_IDs[0]);
            GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4);

            GL.Uniform1(uTextureSelectLocation, 1);
            RenderModel(uModel, mArmadilloManModel, mArmadilloManModelUtility, mVAO_IDs[1]);
            GL.Uniform1(uTextureSelectLocation, 2);
            RenderModel(uModel, mCylinderModel, mCylinderModelUtility, mVAO_IDs[2]);
            GL.Uniform1(uTextureSelectLocation, 3);
            RenderModel(uModel, mBallModel, mBallModelUtility, mVAO_IDs[3]);

            GL.BindVertexArray(0);
            this.SwapBuffers();
        }
        /// <summary>
        /// Loads the models into the scene
        /// </summary>
        /// <param name="uModel"></param>
        /// <param name="model"> Model being loaded </param>
        /// <param name="modelUtility"></param>
        /// <param name="VAO"></param>
        private void RenderModel(int uModel, Matrix4 model, ModelUtility modelUtility, int VAO)
        {
            Matrix4 m = model * mGroundModel;
            uModel = GL.GetUniformLocation(mShader.ShaderProgramID, "uModel");
            GL.UniformMatrix4(uModel, true, ref m);

            GL.BindVertexArray(VAO);
            GL.DrawElements(PrimitiveType.Triangles, modelUtility.Indices.Length, DrawElementsType.UnsignedInt, 0);
        }
        /// <summary>
        /// Removes all data used from the graphics card
        /// </summary>
        /// <param name="e"></param>
        protected override void OnUnload(EventArgs e)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.DeleteBuffers(mVBO_IDs.Length, mVBO_IDs);
            GL.DeleteVertexArrays(mVAO_IDs.Length, mVAO_IDs);
            GL.DeleteTextures(4, mTexture_IDs);
            mShader.Delete();
            base.OnUnload(e);
        }
    }
}
