# com.agraris.review
 Easy implementation of Google Play In-App Review for Unity.

## Usage
``` C#
using Agraris.Tools;

void Awake()
{
    // Init
    IAReview.InitReview();

    // Enable logging
    IAReview.instance.enableLog = true;
}

void LevelComplete()
{
    // Show Review on LevelComplete
    IAReview.instance.ShowReview(int playFrequency, int dayFrequency, ref DateTime lastShowDate);

    // playFrequency => How many session should player play before showing the review
    // dayFrequency => How many days since lastShowDate review dialog will show
    // lastShowDate => The date of review dialog was last shown. This should be saved to the game save.
}
```