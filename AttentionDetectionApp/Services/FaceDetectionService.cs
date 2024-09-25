using AttentionDetectionApp.Models;
using AttentionDetectionApp.Services.Interfaces;
using DlibDotNet;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Collections.Generic;

namespace AttentionDetectionApp.Services
{
    public class FaceDetectionService : IFaceDetectionService
    {
        private readonly ShapePredictor _shapePredictor;
        private readonly FrontalFaceDetector _faceDetector;

        public FaceDetectionService()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            string shapePredictorPath = Path.Combine(baseDirectory, "Data", "shape_predictor_68_face_landmarks.dat");

            _shapePredictor = ShapePredictor.Deserialize(shapePredictorPath);

            _faceDetector = Dlib.GetFrontalFaceDetector();
        }

        public FaceDetectionResult DetectFaceAndAttributes(byte[] frameData)
        {
            using (var img = ConvertByteArrayToImage(frameData))
            {
                var faces = _faceDetector.Operator(img);
                if (faces.Length > 0)
                {
                    var face = faces[0];
                    var shape = _shapePredictor.Detect(img, face);


                    var leftEyeOpenProbability = CalculateEyeOpenProbability(shape, 37, 41); // Ліве око
                    var rightEyeOpenProbability = CalculateEyeOpenProbability(shape, 43, 47); // Праве око

                    var headRotationYaw = CalculateHeadYaw(shape);
                    var headRotationPitch = CalculateHeadPitch(shape);
                    var headRotationRoll = CalculateHeadRoll(shape);


                    var landmarkPoints = GetFacialLandmarkPoints(shape);

                    return new FaceDetectionResult
                    {
                        IsFaceDetected = true,
                        LeftEyeOpenProbability = leftEyeOpenProbability,
                        RightEyeOpenProbability = rightEyeOpenProbability,
                        HeadRotationAngleYaw = headRotationYaw,
                        HeadRotationAnglePitch = headRotationPitch,
                        HeadRotationAngleRoll = headRotationRoll,
                        LandmarkPoints = landmarkPoints
                    };
                }

                return new FaceDetectionResult { IsFaceDetected = false };
            }
        }

        private Dictionary<int, DlibDotNet.Point> GetFacialLandmarkPoints(FullObjectDetection shape)
        {
            var points = new Dictionary<int, DlibDotNet.Point>();

            for (int i = 0; i < shape.Parts; i++)
            {
                var point = shape.GetPart((uint)i);
                points.Add(i, point); 
            }

            return points;
        }

        private Array2D<RgbPixel> ConvertByteArrayToImage(byte[] frameData)
        {
            using (var ms = new MemoryStream(frameData))
            {
                using (var bitmap = new Bitmap(ms))
                {
                    return Dlib.LoadImageData<RgbPixel>(ConvertBitmapToByteArray(bitmap), (uint)bitmap.Height, (uint)bitmap.Width, (uint)(bitmap.Width * 3));
                }
            }
        }

        private byte[] ConvertBitmapToByteArray(Bitmap bitmap)
        {
            BitmapData bmpData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            int numBytes = bmpData.Stride * bitmap.Height;
            byte[] imageData = new byte[numBytes];
            System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, imageData, 0, numBytes);
            bitmap.UnlockBits(bmpData);

            return imageData;
        }

        private double CalculateEyeOpenProbability(FullObjectDetection shape, int eyeTopIndex, int eyeBottomIndex)
        {
            var topPoint = shape.GetPart((uint)eyeTopIndex); 
            var bottomPoint = shape.GetPart((uint)eyeBottomIndex); 

            double eyeHeight = CalculateEuclideanDistance(topPoint, bottomPoint);

            double threshold = 10.0; 
            return eyeHeight < threshold ? 0.0 : 1.0; 
        }




        private double CalculateHeadYaw(FullObjectDetection shape)
        {
            var noseTip = shape.GetPart(30);
            var chin = shape.GetPart(8);

            var dx = chin.X - noseTip.X;
            return dx / 50.0;
        }

        private double CalculateHeadPitch(FullObjectDetection shape)
        {
            var forehead = shape.GetPart(27);
            var chin = shape.GetPart(8);

            var dy = chin.Y - forehead.Y;
            return dy / 50.0;
        }

        private double CalculateHeadRoll(FullObjectDetection shape)
        {
            var leftEye = shape.GetPart(36);
            var rightEye = shape.GetPart(45);

            var dy = rightEye.Y - leftEye.Y;
            var dx = rightEye.X - leftEye.X;

            return System.Math.Atan2(dy, dx) * (180.0 / System.Math.PI);
        }

        private double CalculateEuclideanDistance(DlibDotNet.Point p1, DlibDotNet.Point p2)
        {
            return System.Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y));
        }
    }
}
