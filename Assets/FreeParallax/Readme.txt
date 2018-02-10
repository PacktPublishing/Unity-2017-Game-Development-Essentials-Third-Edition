FreeParallax 1.1.2, by Jeff Johnson, Founder of Digital Ruby, LLC (c) 2015

Greetings! FreeParallax represents many hours of work debugging and refactoring and fine tuning, and I hope you find it useful for your game.
I reallly appreciate feedback, please email me at jjxtra@gmail.com if you have any questions, feedback or bug reports.
For the demo project, you can use the left and right arrow keys to move the parallax.

License: MIT License - https://en.wikipedia.org/wiki/MIT_License
This basically means you can do whatever you want with the source code, provided you keep the comments at the top and don't claim ownership. I'm not responsible if your software explodes, etc.

----------------------------------------------------
Instructions:
You only need the FreeParallax.cs file to run the parallax. Feel free to delete the demo scene and demo script along with the demo images if you don't want them.
Free Parallax uses a list of parallax elements / layers to do it's work. These elements are highly configurable.

Before we get started, please read this following important information which will save you a lot of grief and debugging time:
*** IMPORTANT ***
All game objects in the parallax must have a renderer
Never re-use game objects in multiple places in the parallax. Create clones of your objects instead and use those.

Ok, now that we have gotten that out of the way, here's everything you need to use Free Parallax:

Step by Step Guide:
- Import Free Parallax asset into your project
- Create an empty game object
- Drag the FreeParallax.cs script on to the new game object for use as a game component
- Import the images you want to use in the parallax by dragging them into the Unity assets area
- Drag each image one at a time and put it underneath the empty game object in the hieararchy
- Time to setup the parallax script, click on the empty game object so that the script appears in the game object properties
- Change the speed based on your game requirements. If you are controlling a character like a platformer, set the speed to 0 and change the speed in code based on character movement (see the FreeParallaxDemo.cs file for an example).
- Set the size of the elements list on the script to the number of layers in your parallax. Notice that the Unity editor expands the list to the size you typed in (just type in the size and press ENTER).
Note: Each element must have 1 or more game objects in it
Note: You must never re-use game objects in the parallax, instead create a second clone object if you want to re-use the same graphic
- Set the size of the game object list to the number of game objects in the layer
- Draw a game object from the hierarchy into the game object slots in the list
- Change the speed ratio to something appropriate (typically foreground elements will have a speed ratio closer to 1, while background elements (such as clouds or mountains) will have low speed ratios, such as 0.05.
- Setup the repositioning logic. The reposition mode contains 4 possible options:
   Wrap Anchor Bottom : The layer will be seamless and fill the entire width (or height if vertical) and be anchored to the bottom of the screen
   Wrap Anchor Top : The layer will be seamless and fill the entire width (or height if vertical) and be anchored to the top of the screen
   Wrap Anchor None : The layer will be seamless and fill the entire width (or height if vertical) and maintain the original x or y position
   Individual Start On Screen : The layer contains individual objects, and they all start wherever they are placed in the scene designer
   Individual Start Off Screen : The layer contains individual objects, and they start off screen
Note: For the wrapping modes, only one graphic is required. Free Parallax will add a second graphic for you automatically to ensure a seamless parallax
Note: You may add as many graphics as you want for the wrap mode in case you want a very dynamic, long, unique scrolling background
- Set the scale height (as a percentage of screen height or width if vertical)
Note: For full size wrapping elements, 1 is a good value as it ensures the graphic is the same height as the screen
Note: For wrapping elements that are smaller than the screen height (or width if vertical), this value can be set to something smaller than 1, like a road or river
Note: The scale height may be left as 0, in which case the scale is not modified on the graphic and whatever you set in the scene view is used
Note: For individual objects, the scale height should likely be smaller than 1
- If you want to maintain the sort order in the parallax script, change the sort order to non-zero. Higher sort orders are closer to the camera.
- For layers with individual objects, you can set the min and max x y percents to ensure that the random placement of the object stays in the layer
Note: These four properties are ignored for layers that wrap
   Min x percent : The minimum number of screen widths objects will start at again when they wrap off the edge
   Max x percent : The maximum number of screen widths objects will start at again when they wrap off the edge
   Min y percent : The minimum percent of screen height that the object will be placed at, using the anchor point of the object
   Max y percent : The maximum percent of screen height that the object will be placed at, using the anchor point of the object
- For custom repositioning logic and complex requirements, you can instead use the RepositionLogicFunction property in code, which allows you to set the position of the object when it goes off screen
- Remember, for a vertical parallax, turn the IsHorizontal property to false
----------------------------------------------------

Have a wonderful day!

- Jeff
jjxtra@gmail.com