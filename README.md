# URP-SharpShadow

<img src="/../pics/pics/Main.jpg" width="100%" height="100%"></img>

How to preview
-----------
* Install [Universal Render Pipeline](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@latest/).
* Download and import the [Unity package](https://github.com/malyawka/URP-SharpShadow/releases/tag/Unity).
* Open scene from <b>Assets/PolygonStarter/Scenes/Demo.unity</b>.

<b>Tested with</b>
Unity version - 2020.3
URP version - 10.7

Parameters for URP Feature
-----------
<img src="/../pics/pics/URP.jpg" width="75%" height="75%"></img>
* <b>General</b>:
  * <b>Render Pass Event</b> - controls when the render pass executes.
  * <b>Render Layer</b> - controls which layer shadows are rendered on (default is TransparentFX).
* <b>Shadows</b>:
  * <b>Static Shadows</b> - render static shadows (enabled/disabled).
  * <b>Planar & Active Shadows</b> - render planar or active shadows (disabled/planar/active).
  * <b>Intensity</b> - shadow intensivity (alpha from 0 to 1).
  * <b>Floor Height</b> - the height of the floor on which the shadow will be cast (actual for planar and active shadows).
  * <b>Near Extrude</b> - the distance the volume shadow will move relative to the original mesh (actual for active shadows).
  * <b>Far Extrude</b> - the distance that the volume shadow will move relative to the floor (actual for active shadows).

Parameters for Static Shadows
-----------
<img src="/../pics/pics/Static.jpg" width="75%" height="75%"></img>
* <b>Ground</b> - floor height in world coordinates (mesh will be extruded to this height).
* <b>Offset</b> - offset of the interfering volume of shadows relative to the original.
* <b>Reverse</b> - changes the order of the triangles (may be necessary if you need to turn the mesh inside out).

Parameters for Active & Skin Shadows
-----------
<img src="/../pics/pics/Active.jpg" width="75%" height="75%"></img>
* <b>Bounds Factor</b> - increases the bounds for the volume shadow mesh.
* <b>Disable Shadows</b> - disables the shadow display for a specific object.

How To Use
-----------
* Select object with Mesh Renderer or Skinned Mesh Rrenderer.
* Then "Add Component" -> "MalyaWka" -> "SharpShadow".
  * "Static" for objects that will not rotate and move along the Y axis.
  * "Active" for Mesh Renderers that will move and rotate.
  * "Skin" for Skinned Mesh Renderer.
* Click "Create" to create the shadow.
* Click "Remove" to remove the shadow (attention, before removing the component from the object, be sure to click the "Remove" button).

Notes
------
* As an example, use the free [POLYGON Starter Pack](https://assetstore.unity.com/packages/3d/props/polygon-starter-pack-low-poly-3d-art-by-synty-156819) asset from reputable [Synty Studios](https://assetstore.unity.com/publishers/5217).
* If you are not familiar with the Universal Render Pipeline, you can find the [official tutorial here](https://learn.unity.com/tutorial/introduction-to-urp#).
* Many thanks to [Jim Cheng](https://github.com/chengkehan) for his [Shadow Volume](https://github.com/chengkehan/ShadowVolume)
* Thanks to [Roman Fedorov](https://github.com/studentutu) for saving Gustav Olsson's [source codes](https://github.com/studentutu/shadow-volumes-toolkit).

Good to everyone!:v:
