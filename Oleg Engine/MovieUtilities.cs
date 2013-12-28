using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;


namespace OlegEngine
{
    /// <summary>
    /// Hold a bunch of utilities that makes it easy to export screenshots or ingame recordings
    /// </summary>
    public class MovieUtilities
    {
        /// <summary>
        /// The relative directory to the screenshots folder
        /// </summary>
        public const string ScreenshotsDir = "Screenshots/";
        /// <summary>
        /// The relative directory to the image sequences folder
        /// </summary>
        public const string SequenceDir = ScreenshotsDir + "Movies/";

        /// <summary>
        /// Set if a movie is currently being recorded
        /// </summary>
        public static bool IsRecordingMovie { get; private set; }
        /// <summary>
        /// If a movie is being recorded, these are its current settings
        /// </summary>
        public static MovieSettings CurrentMovieSettings;

        /// <summary>
        /// The size, in pixels, of the 'recording' dot status on the screen
        /// Note it does not appear on the final movie
        /// </summary>
        public static int RecordDotSize = 64;

        /// <summary>
        /// The padding from the edges for the recording status dot.
        /// </summary>
        public static int RecordDotPadding = 10;

        private static uint movieFrameNumber = 0;
        private static Bitmap movieBitmap;
        private static int recordDotTexture = -1;

        /// <summary>
        /// Grab a screenshot of the screen at this moment of rendering
        /// </summary>
        /// <returns>The System.Drawing.Bitmap object representing the screenshot</returns>
        public static Bitmap GrabScreenshot()
        {
            if (GraphicsContext.CurrentContext == null)
                throw new GraphicsContextMissingException();

            Bitmap bmp = new Bitmap(Utilities.engine.ClientSize.Width, Utilities.engine.ClientSize.Height);
            System.Drawing.Imaging.BitmapData data =
                bmp.LockBits(Utilities.engine.ClientRectangle, System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            GL.ReadPixels(0, 0, Utilities.engine.ClientSize.Width, Utilities.engine.ClientSize.Height, PixelFormat.Bgr, PixelType.UnsignedByte, data.Scan0);
            bmp.UnlockBits(data);

            bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
            return bmp;
        }

        /// <summary>
        /// Save a screenshot of the current screen to file
        /// </summary>
        /// <param name="filename">The filename of the screenshot to be saved</param>
        /// <returns>If the screen was successfully captured and file successfully saved</returns>
        public static bool SaveScreenshot(string filename, string extension = ".png")
        {
            try
            {
                Bitmap ss = GrabScreenshot();

                if (ss != null)
                {
                    ss.Save(ScreenshotsDir + filename + extension);
                    Utilities.Print("Saved screenshot \"{0}{1}\"!", Utilities.PrintCode.INFO, filename, extension);
                    return true;
                }

            }
            catch (Exception ex)
            {
                Utilities.Print("Failed to save screenshot \"{0}{1}\". {2}", Utilities.PrintCode.WARNING, filename,extension, ex.Message);
            }

            return false;
        }

        /// <summary>
        /// Begin a movie with specified settings
        /// Movies will capture each rendered frame, so it WILL slow down the game
        /// See: Utilities.ShouldForceFrametime
        /// </summary>
        /// <param name="settings"></param>
        public static void StartMovie(MovieSettings settings)
        {
            movieFrameNumber = 0;
            CurrentMovieSettings = settings;
            try
            {
                //If it's an image sequence, create a folder that will hold all the frames. everything else is one compiled video so we don't need to
                if (settings.Format == MovieType.ImageSequence)
                {
                    Directory.CreateDirectory(SequenceDir + settings.Filename);
                }

                //To make this go faster we're going to reuse the same bitmap object
                movieBitmap = new Bitmap(Utilities.engine.ClientSize.Width, Utilities.engine.ClientSize.Height);

                //Create our recording material, if neccessary
                if (recordDotTexture == -1)
                    recordDotTexture = GenerateRecordDotTexture();
            }
            catch (Exception ex)
            {
                Utilities.Print("Unable to start recording movie \"{0}\". {1}", Utilities.PrintCode.WARNING, settings.Filename, ex.Message);
                return;
            }

            //Hook into the engine rendering
            Utilities.engine.OnFrameFinish -= OnDraw; //just in case teeHEEBADCODE
            Utilities.engine.OnFrameFinish += OnDraw;

            IsRecordingMovie = true;

            Utilities.Print("Started recording moving \"{0}\"!", Utilities.PrintCode.INFO, settings.Filename);
        }

        /// <summary>
        /// If a movie is currently being recorded, stop recording it.
        /// </summary>
        public static void EndMovie()
        {
            //Unsubscribe from engine drawing
            Utilities.engine.OnFrameFinish -= OnDraw;
            IsRecordingMovie = false;

            Utilities.Print("Finished recording movie! {0} frames.", Utilities.PrintCode.INFO, movieFrameNumber.ToString());
        }

        private static void OnDraw()
        {
            //Make sure these are valid. they are kind of important
            if (movieBitmap == null || GraphicsContext.CurrentContext == null)
            {
                Utilities.Print("Failed to start movie.", Utilities.PrintCode.ERROR);
                Utilities.Print("{0}MovieUtilities.movieBitmap = ", Utilities.PrintCode.ERROR, movieBitmap.ToString());
                Utilities.Print("{0}GraphicsContext.CurrentContext = ", Utilities.PrintCode.ERROR, GraphicsContext.CurrentContext.ToString());

                EndMovie();
            } 

            //Because GrabScreenshot is slower, we'll have to recreate functionality
            System.Drawing.Imaging.BitmapData data = movieBitmap.LockBits(Utilities.engine.ClientRectangle, System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            GL.ReadPixels(0, 0, Utilities.engine.ClientSize.Width, Utilities.engine.ClientSize.Height, PixelFormat.Bgr, PixelType.UnsignedByte, data.Scan0);
            movieBitmap.UnlockBits(data);
            movieBitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);

            if (CurrentMovieSettings.Format == MovieType.ImageSequence)
            {
                try
                {
                    movieBitmap.Save(string.Format("{0}{1}/{2}{3}.bmp", SequenceDir, CurrentMovieSettings.Filename, CurrentMovieSettings.Filename, movieFrameNumber.ToString()));
                }
                catch (Exception ex)
                {
                    Utilities.Print("Failed to save frame. {0}", Utilities.PrintCode.ERROR, ex.Message);
                }
            }


            //Since we've already grabbed the frame, let's draw a SUPER SICK recording dot in the upper right corner
            Graphics.EnableBlending(true);
            GUI.Surface.SetDrawColor(Color.White);
            GUI.Surface.SetTexture(recordDotTexture);
            GUI.Surface.DrawRect(Utilities.engine.Width - RecordDotSize - RecordDotPadding, RecordDotPadding, RecordDotSize, RecordDotSize);
            Graphics.EnableBlending(false);

            //Another frame another...NICKEL
            movieFrameNumber++;
        }

        private static int GenerateRecordDotTexture()
        {
            Bitmap tex = new Bitmap(RecordDotSize, RecordDotSize);
            tex.MakeTransparent();
            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(tex))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.FillEllipse(System.Drawing.Brushes.Red, 0, 0, RecordDotSize, RecordDotSize);
                g.Flush();
            }

            int texture = Utilities.LoadTexture(tex);

            //If it didn't load it right, resort to the missing texture texture
            texture = texture == -1 ? Utilities.ErrorTex : texture;

            //Dispose of this mess
            tex.Dispose();

            return texture;
        }
    }

    /// <summary>
    /// The different formats to export the movie.
    /// This will be changed soon
    /// </summary>
    public enum MovieType
    {
        ImageSequence,
        AVI,
        Webm,
        MP4,
    }

    /// <summary>
    /// Struct to hold information about how the movie will be saved to disk
    /// </summary>
    public struct MovieSettings
    {
        /// <summary>
        /// The filename of the movie
        /// </summary>
        public string Filename;
        /// <summary>
        /// The output format of the movie
        /// </summary>
        public MovieType Format;

        /// <summary>
        /// Create a new movie settings object with specified settings
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="type"></param>
        public MovieSettings(string filename, MovieType type)
        {
            Filename = filename;
            Format = type;
        }
    }
}
