using Accord;
using Accord.Imaging;
using Accord.Imaging.Filters;
using Accord.Math.Distances;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Capturer.Imaging
{
    public class PanoramicBlender
    {
        List<Bitmap> _images;

        public PanoramicBlender()
        {
            _images = new List<Bitmap>();
        }

        public void AddImage(Bitmap bmp)
        {
            _images.Add(bmp);
        }

        public Bitmap GetBlendedImage()
        {
            var groups = GroupImagesInPairs(_images);

            var bitmapsToDispose = new List<Bitmap>();

            while (groups.Count >= 2)
            {
                groups = BlendImageGroups(groups);

                // don't forget to call Dispose() on the newly created bitmaps by the blending process
                // but we should keep the original images, so that we can add another
                // image to them and perform the blending of all images again
                bitmapsToDispose.AddRange(groups.SelectMany(g => g));
            }
            var blendedFinalImage = BlendImages(groups[0][0], groups[0][1]);

            bitmapsToDispose.ForEach(bmp => bmp.Dispose());

            return blendedFinalImage;
        }

        private List<List<Bitmap>> BlendImageGroups(List<List<Bitmap>> groups)
        {
            var blendedGroups = new List<List<Bitmap>>();
            int num = 0;
            foreach (var group in groups)
            {
                if (num < groups.Count - 1)
                {
                    var blendedImg1 = BlendImages(group[0], group[1]);
                    var blendedImg2 = BlendImages(groups[num + 1][0], groups[num + 1][1]);
                    blendedGroups.Add(new List<Bitmap> { blendedImg1, blendedImg2 });
                }
                num++;
            }
            return blendedGroups;
        }

        private Bitmap BlendImagesSurf(Bitmap img1, Bitmap img2)
        {
            // Step 1: Detect feature points using Surf Corners Detector
            SpeededUpRobustFeaturesDetector surf = new SpeededUpRobustFeaturesDetector();

            var surfPoints1 = surf.ProcessImage(img1).ToArray();
            var surfPoints2 = surf.ProcessImage(img2).ToArray();

            // Step 2: Match feature points using a k-NN
            KNearestNeighborMatching matcher = new KNearestNeighborMatching(5);
            IntPoint[][] matches = matcher.Match(surfPoints1, surfPoints2);

            // Get the two sets of points
            var correlationPoints1 = matches[0];
            var correlationPoints2 = matches[1];

            // Step 3: Create the homography matrix using a robust estimator
            RansacHomographyEstimator ransac = new RansacHomographyEstimator(0.001, 0.99);
            var homography = ransac.Estimate(correlationPoints1, correlationPoints2);

            // Step 4: Project and blend the second image using the homography
            Blend blend = new Blend(homography, img1);
            return blend.Apply(img2);

        }

        private Bitmap BlendImagesFreak(Bitmap img1, Bitmap img2)
        {
            // Step 1: Detect feature points using FREAK Features Detector
            FastRetinaKeypointDetector freak = new FastRetinaKeypointDetector();

            var keyPoints1 = freak.ProcessImage(img1).ToArray();
            var keyPoints2 = freak.ProcessImage(img2).ToArray();

            // Step 2: Match feature points using a k-NN
            var matcher = new KNearestNeighborMatching<byte[]>(5, new Hamming());
            IntPoint[][] matches = matcher.Match(keyPoints1, keyPoints2);

            // Get the two sets of points
            var correlationPoints1 = matches[0];
            var correlationPoints2 = matches[1];

            // Step 3: Create the homography matrix using a robust estimator
            RansacHomographyEstimator ransac = new RansacHomographyEstimator(0.001, 0.99);
            var homography = ransac.Estimate(correlationPoints1, correlationPoints2);


            // Step 4: Project and blend the second image using the homography
            Blend blend = new Blend(homography, img1);
            return blend.Apply(img2);
        }

        private Bitmap BlendImages(Bitmap bmp1, Bitmap bmp2)
        {
            // Step 1: Detect feature points using Harris Corners Detector
            HarrisCornersDetector harris = new HarrisCornersDetector(0.001f, 1000f);
            var harrisPoints1 = harris.ProcessImage(bmp1).ToArray();
            var harrisPoints2 = harris.ProcessImage(bmp2).ToArray();

            // Step 2: Match feature points using a correlation measure
            CorrelationMatching matcher = new CorrelationMatching(3, bmp1, bmp2); // it was 9
            Accord.IntPoint[][] matches = matcher.Match(harrisPoints1, harrisPoints2);

            // Get the two sets of points
            var correlationPoints1 = matches[0];
            var correlationPoints2 = matches[1];

            // Step 3: Create the homography matrix using a robust estimator
            RansacHomographyEstimator ransac = new RansacHomographyEstimator(0.001, 0.5); // probability was 0.99
            var homography = ransac.Estimate(correlationPoints1, correlationPoints2);

            // Step 4: Project and blend the second image using the homography
            Blend blend = new Blend(homography, bmp1);
            var blendedImg1 = blend.Apply(bmp2);

            return blendedImg1;
        }

        private List<List<Bitmap>> GroupImagesInPairs(List<Bitmap> images)
        {
            var groups = new List<List<Bitmap>>();

            if (_images.Count <= 1)
                return groups;

            int num = 0;
            foreach (var image in _images)
            {
                if (num < _images.Count - 1)
                {
                    groups.Add(new List<Bitmap> { image, _images[num + 1] });
                }
                num++;
            }

            return groups;
        }
    }
}
