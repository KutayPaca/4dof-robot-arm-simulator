using System;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace RobotKoluSimulasyonu
{
    public class RobotWindow : GameWindow
    {
        // Robot kolu parametreleri
        private float theta1 = 0f;      // Taban dönüşü (Y ekseni)
        private float theta2 = 0f;      // Omuz (X ekseni)
        private float theta3 = 0f;      // Dirsek (X ekseni)
        private float theta4 = 0f;      // Bilek/Roll (Z ekseni) - YENİ 4. EKLEM
        
        // Kol uzunlukları
        private const float L1 = 2.0f;  // Taban-Omuz arası
        private const float L2 = 1.5f;  // Omuz-Dirsek arası
        private const float L3 = 1.0f;  // Dirsek-Uç efektör arası
        
        // Pençe durumu
        private bool gripperOpen = false;
        private float gripperAngle = 0f; // Pençe açıklığı
        
        // Uç efektör koordinatları
        private float endEffectorX = 0f;
        private float endEffectorY = 0f;
        private float endEffectorZ = 0f;
        private float totalReach = 0f;
        
        // Kamera kontrolü
        private float cameraAngleX = 20f;
        private float cameraAngleY = 45f;
        private float cameraDistance = 10f;

        public RobotWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            
            Console.WriteLine("OpenGL Version: " + GL.GetString(StringName.Version));
            Console.WriteLine("OpenGL Renderer: " + GL.GetString(StringName.Renderer));
            Console.WriteLine("OpenGL Vendor: " + GL.GetString(StringName.Vendor));
            
            GL.ClearColor(0.1f, 0.1f, 0.15f, 1.0f);
            GL.Enable(EnableCap.DepthTest);
            
            Console.WriteLine("OpenGL Yüklendi! Robot görünmelidir.");
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            
            GL.Viewport(0, 0, e.Width, e.Height);
            Console.WriteLine($"Pencere boyutu: {e.Width}x{e.Height}");
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
            
            var keyboard = KeyboardState;
            
            if (keyboard.IsKeyDown(Keys.Escape))
            {
                Close();
            }
            
            // Robot kontrolleri
            float rotationSpeed = 60f * (float)args.Time; // Derece/saniye
            
            // Theta1 (Taban dönüşü) - Q/E tuşları
            if (keyboard.IsKeyDown(Keys.Q))
                theta1 += rotationSpeed;
            if (keyboard.IsKeyDown(Keys.E))
                theta1 -= rotationSpeed;
            
            // Theta2 (Omuz) - W/S tuşları ile sınırlama
            if (keyboard.IsKeyDown(Keys.W))
                theta2 += rotationSpeed;
            if (keyboard.IsKeyDown(Keys.S))
                theta2 -= rotationSpeed;
            
            // Theta3 (Dirsek) - A/D tuşları ile sınırlama
            if (keyboard.IsKeyDown(Keys.A))
                theta3 += rotationSpeed;
            if (keyboard.IsKeyDown(Keys.D))
                theta3 -= rotationSpeed;
            
            // Theta4 (Bilek/Roll) - R/F tuşları (sınırsız 360° dönüş)
            if (keyboard.IsKeyDown(Keys.R))
                theta4 += rotationSpeed;
            if (keyboard.IsKeyDown(Keys.F))
                theta4 -= rotationSpeed;
            
            // ZORUNLU GEREKSINIM 1: Eklem Sınırlandırması [-90, 90]
            theta2 = MathHelper.Clamp(theta2, -90f, 90f);
            theta3 = MathHelper.Clamp(theta3, -90f, 90f);
            // Theta4 sınırsız - 360° dönebilir
            
            // Pençe kontrolü - X/Z tuşları
            if (keyboard.IsKeyPressed(Keys.X))
            {
                gripperOpen = !gripperOpen;
            }
            
            // Pençe animasyonu
            float targetGripperAngle = gripperOpen ? 30f : 0f;
            gripperAngle += (targetGripperAngle - gripperAngle) * 5f * (float)args.Time;
            
            // Kamera kontrolleri
            if (keyboard.IsKeyDown(Keys.Up))
                cameraAngleX += 30f * (float)args.Time;
            if (keyboard.IsKeyDown(Keys.Down))
                cameraAngleX -= 30f * (float)args.Time;
            if (keyboard.IsKeyDown(Keys.Left))
                cameraAngleY += 30f * (float)args.Time;
            if (keyboard.IsKeyDown(Keys.Right))
                cameraAngleY -= 30f * (float)args.Time;
            if (keyboard.IsKeyDown(Keys.PageUp))
                cameraDistance -= 5f * (float)args.Time;
            if (keyboard.IsKeyDown(Keys.PageDown))
                cameraDistance += 5f * (float)args.Time;
            
            cameraDistance = MathHelper.Clamp(cameraDistance, 5f, 20f);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            
            // Her şeyi temizle
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
            // Projection matrisini ayarla
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            
            float aspectRatio = (float)ClientSize.X / ClientSize.Y;
            float fov = 45f;
            float near = 0.1f;
            float far = 100f;
            
            float top = near * (float)Math.Tan(MathHelper.DegreesToRadians(fov) / 2.0);
            float bottom = -top;
            float right = top * aspectRatio;
            float left = -right;
            
            GL.Frustum(left, right, bottom, top, near, far);
            
            // ModelView matrisini ayarla
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            
            // Kamera transformasyonu
            GL.Translate(0f, -2f, -cameraDistance);
            GL.Rotate(cameraAngleX, 1f, 0f, 0f);
            GL.Rotate(cameraAngleY, 0f, 1f, 0f);
            
            // Zemin çizimi
            DrawGround();
            
            // Eksenleri çiz
            DrawAxes();
            
            // Robot kolunu çiz
            DrawRobotArm();
            
            SwapBuffers();
        }

        private void DrawGround()
        {
            GL.Begin(PrimitiveType.Lines);
            GL.Color3(0.3f, 0.3f, 0.3f);
            
            for (int i = -10; i <= 10; i++)
            {
                GL.Vertex3(i, 0, -10);
                GL.Vertex3(i, 0, 10);
                GL.Vertex3(-10, 0, i);
                GL.Vertex3(10, 0, i);
            }
            
            GL.End();
        }

        private void DrawAxes()
        {
            GL.Begin(PrimitiveType.Lines);
            
            // X ekseni (Kırmızı)
            GL.Color3(1f, 0f, 0f);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(2, 0, 0);
            
            // Y ekseni (Yeşil)
            GL.Color3(0f, 1f, 0f);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(0, 2, 0);
            
            // Z ekseni (Mavi)
            GL.Color3(0f, 0f, 1f);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(0, 0, 2);
            
            GL.End();
        }

        private void DrawRobotArm()
        {
            GL.PushMatrix();
            
            // Taban
            GL.Color3(0.5f, 0.5f, 0.5f);
            DrawCylinder(0.5f, 0.3f, 20);
            
            // Theta1 (Taban dönüşü - Y ekseni etrafında)
            GL.Rotate(theta1, 0f, 1f, 0f);
            
            // Link 1 (Taban-Omuz) - Silindir geometrisi
            GL.PushMatrix();
            GL.Color3(0.8f, 0.2f, 0.2f);
            DrawCylinder(0.15f, L1, 16);
            GL.PopMatrix();
            
            // Omuz eklemi
            GL.Translate(0f, L1, 0f);
            GL.Color3(0.6f, 0.6f, 0.6f);
            DrawSphere(0.25f, 20, 20);
            
            // Theta2 (Omuz - X ekseni etrafında)
            GL.Rotate(theta2, 1f, 0f, 0f);
            
            // Link 2 (Omuz-Dirsek) - Silindir geometrisi
            GL.PushMatrix();
            GL.Color3(0.2f, 0.8f, 0.2f);
            DrawCylinder(0.12f, L2, 16);
            GL.PopMatrix();
            
            // Dirsek eklemi
            GL.Translate(0f, L2, 0f);
            GL.Color3(0.6f, 0.6f, 0.6f);
            DrawSphere(0.2f, 20, 20);
            
            // Theta3 (Dirsek - X ekseni etrafında)
            GL.Rotate(theta3, 1f, 0f, 0f);
            
            // Link 3 (Dirsek-Uç efektör) - Silindir geometrisi
            GL.PushMatrix();
            GL.Color3(0.2f, 0.2f, 0.8f);
            DrawCylinder(0.1f, L3, 16);
            GL.PopMatrix();
            
            // Uç efektöre git
            GL.Translate(0f, L3, 0f);
            
            // Theta4 (Bilek/Roll - Z ekseni etrafında) - YENİ 4. EKLEM
            GL.Rotate(theta4, 0f, 1f, 0f);
            
            // ZORUNLU GEREKSINIM 2: Uç efektör koordinatlarını al
            float[] modelMatrixArray = new float[16];
            GL.GetFloat(GetPName.ModelviewMatrix, modelMatrixArray);
            
            // Model-view matrisinden pozisyon bilgisini çıkar
            endEffectorX = modelMatrixArray[12]; // M41
            endEffectorY = modelMatrixArray[13]; // M42
            endEffectorZ = modelMatrixArray[14]; // M43
            
            // ZORUNLU GEREKSINIM 2: Toplam uzunluk hesapla (Euclidean Distance)
            totalReach = (float)Math.Sqrt(endEffectorX * endEffectorX + 
                                          endEffectorY * endEffectorY + 
                                          endEffectorZ * endEffectorZ);
            
            // ZORUNLU GEREKSINIM 3: Pençe Mekanizması
            DrawGripper();
            
            GL.PopMatrix();
            
            // Pencere başlığını güncelle
            UpdateTitle();
        }

        private void DrawGripper()
        {
            // Pençe tabanı
            GL.Color3(0.9f, 0.9f, 0.1f);
            DrawCylinder(0.15f, 0.2f, 12);
            
            GL.Translate(0f, 0.2f, 0f);
            
            // Sol pençe parmağı
            GL.PushMatrix();
            GL.Rotate(-gripperAngle, 0f, 0f, 1f);
            GL.Translate(-0.1f, 0.2f, 0f);
            GL.Color3(0.8f, 0.8f, 0.1f);
            DrawBox(0.08f, 0.4f, 0.08f);
            
            // Sol pençe ucu
            GL.Translate(0f, 0.2f, 0f);
            GL.Rotate(-30f, 0f, 0f, 1f);
            GL.Translate(-0.05f, 0.1f, 0f);
            DrawBox(0.08f, 0.2f, 0.06f);
            GL.PopMatrix();
            
            // Sağ pençe parmağı
            GL.PushMatrix();
            GL.Rotate(gripperAngle, 0f, 0f, 1f);
            GL.Translate(0.1f, 0.2f, 0f);
            GL.Color3(0.8f, 0.8f, 0.1f);
            DrawBox(0.08f, 0.4f, 0.08f);
            
            // Sağ pençe ucu
            GL.Translate(0f, 0.2f, 0f);
            GL.Rotate(30f, 0f, 0f, 1f);
            GL.Translate(0.05f, 0.1f, 0f);
            DrawBox(0.08f, 0.2f, 0.06f);
            GL.PopMatrix();
        }

        private void UpdateTitle()
        {
            // ZORUNLU GEREKSINIM: Tüm bilgileri pencere başlığında göster
            string gripperStatus = gripperOpen ? "Açık" : "Kapalı";
            Title = string.Format(
                "4 DOF Robot Kolu | X: {0:F2} Y: {1:F2} Z: {2:F2} | Toplam Uzunluk: {3:F2} | Pençe: {4} | θ4: {5:F1}° | " +
                "Kontroller: Q/E(Taban) W/S(Omuz) A/D(Dirsek) R/F(Bilek Roll) X(Pençe) Oklar(Kamera)",
                endEffectorX, endEffectorY, endEffectorZ, totalReach, gripperStatus, theta4
            );
        }

        private void DrawCylinder(float radius, float height, int segments)
        {
            GL.Begin(PrimitiveType.TriangleFan);
            GL.Normal3(0f, -1f, 0f);
            GL.Vertex3(0f, 0f, 0f);
            for (int i = 0; i <= segments; i++)
            {
                float angle = 2f * (float)Math.PI * i / segments;
                float x = radius * (float)Math.Cos(angle);
                float z = radius * (float)Math.Sin(angle);
                GL.Vertex3(x, 0f, z);
            }
            GL.End();
            
            GL.Begin(PrimitiveType.TriangleFan);
            GL.Normal3(0f, 1f, 0f);
            GL.Vertex3(0f, height, 0f);
            for (int i = 0; i <= segments; i++)
            {
                float angle = 2f * (float)Math.PI * i / segments;
                float x = radius * (float)Math.Cos(angle);
                float z = radius * (float)Math.Sin(angle);
                GL.Vertex3(x, height, z);
            }
            GL.End();
            
            GL.Begin(PrimitiveType.QuadStrip);
            for (int i = 0; i <= segments; i++)
            {
                float angle = 2f * (float)Math.PI * i / segments;
                float x = radius * (float)Math.Cos(angle);
                float z = radius * (float)Math.Sin(angle);
                GL.Normal3(x / radius, 0f, z / radius);
                GL.Vertex3(x, 0f, z);
                GL.Vertex3(x, height, z);
            }
            GL.End();
        }

        private void DrawBox(float width, float height, float depth)
        {
            float w = width / 2f;
            float h = height / 2f;
            float d = depth / 2f;
            
            GL.Begin(PrimitiveType.Quads);
            
            // Ön yüz
            GL.Normal3(0f, 0f, 1f);
            GL.Vertex3(-w, -h, d);
            GL.Vertex3(w, -h, d);
            GL.Vertex3(w, h, d);
            GL.Vertex3(-w, h, d);
            
            // Arka yüz
            GL.Normal3(0f, 0f, -1f);
            GL.Vertex3(-w, -h, -d);
            GL.Vertex3(-w, h, -d);
            GL.Vertex3(w, h, -d);
            GL.Vertex3(w, -h, -d);
            
            // Üst yüz
            GL.Normal3(0f, 1f, 0f);
            GL.Vertex3(-w, h, -d);
            GL.Vertex3(-w, h, d);
            GL.Vertex3(w, h, d);
            GL.Vertex3(w, h, -d);
            
            // Alt yüz
            GL.Normal3(0f, -1f, 0f);
            GL.Vertex3(-w, -h, -d);
            GL.Vertex3(w, -h, -d);
            GL.Vertex3(w, -h, d);
            GL.Vertex3(-w, -h, d);
            
            // Sağ yüz
            GL.Normal3(1f, 0f, 0f);
            GL.Vertex3(w, -h, -d);
            GL.Vertex3(w, h, -d);
            GL.Vertex3(w, h, d);
            GL.Vertex3(w, -h, d);
            
            // Sol yüz
            GL.Normal3(-1f, 0f, 0f);
            GL.Vertex3(-w, -h, -d);
            GL.Vertex3(-w, -h, d);
            GL.Vertex3(-w, h, d);
            GL.Vertex3(-w, h, -d);
            
            GL.End();
        }

        private void DrawSphere(float radius, int slices, int stacks)
        {
            for (int i = 0; i < stacks; i++)
            {
                float lat0 = (float)Math.PI * (-0.5f + (float)i / stacks);
                float z0 = radius * (float)Math.Sin(lat0);
                float zr0 = radius * (float)Math.Cos(lat0);
                
                float lat1 = (float)Math.PI * (-0.5f + (float)(i + 1) / stacks);
                float z1 = radius * (float)Math.Sin(lat1);
                float zr1 = radius * (float)Math.Cos(lat1);
                
                GL.Begin(PrimitiveType.QuadStrip);
                for (int j = 0; j <= slices; j++)
                {
                    float lng = 2f * (float)Math.PI * (float)j / slices;
                    float x = (float)Math.Cos(lng);
                    float y = (float)Math.Sin(lng);
                    
                    GL.Normal3(x * zr0, y * zr0, z0);
                    GL.Vertex3(x * zr0, z0, y * zr0);
                    GL.Normal3(x * zr1, y * zr1, z1);
                    GL.Vertex3(x * zr1, z1, y * zr1);
                }
                GL.End();
            }
        }
    }
}
