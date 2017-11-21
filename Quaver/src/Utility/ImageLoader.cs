﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace Quaver.Utility
{
    internal class ImageLoader
    {
        /// <summary>
        ///     Loads an image into a Texture2D
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal static Texture2D Load(string path)
        {
            try
            {
                using (var fileStream = new FileStream(path, FileMode.Open))
                {
                    return Texture2D.FromStream(GameBase.GraphicsDevice, fileStream);
                }
            }
            catch (Exception e)
            {
                return new Texture2D(GameBase.GraphicsDevice, 1280, 720);
            }
        }
    }
}
