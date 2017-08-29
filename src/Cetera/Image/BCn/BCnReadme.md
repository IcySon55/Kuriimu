BCn
===================

BCn is a C# port of some of the BCn encoders in the [DirectXTex texture processing library](http://directxtex.codeplex.com/). It supports encoding to BC1, BC2, BC3, BC4, and BC5.

The library is highly modular. Its core works with a single block at once, making it trivial to integrate with customized image formats.

The Block Types
===================

The library defines a simple block structure for each block type. Each of these structs represents a single 4x4 block of image data. These blocks are what the block encoders return. The block types fall into two categories.

Primitive Blocks
-------------------
The primitive blocks contain raw encoded pixel data:

* `BC1Block`: encodes BC1 or BC1A data. (The encodings are identical, so only a single type is provided for the two.)
* `BC2ABlock`: encodes the alpha portion of a BC2 block. This is basically just a 4-bit alpha value.
* `BC4UBlock`: encodes a single channel of *unsigned* (that is, in the range [0, 1]) values.
* `BC4SBlock`: encodes a single channel of *signed* (that is, in the range [-1, 1]) values.

The primitive block types all contain a `PackedValue` member, which can be used to get or set the raw image bits.

Composite Blocks
-------------------
The composite block types combine two primitive blocks together into a single block type.

* `BC2Block`: combines a `BC1Block` (for RGB) and a `BC2ABlock` (for alpha). It's not enforced at the type level, but the RGB component of these blocks should not contain alpha data.
* `BC3Block`: Combines a `BC1Block` (for RGB) with a `BC4UBlock` (for alpha). Again, while it is not enforced at the type level, the RGB part should contain no alpha data.
* `BC5UBlock`, `BC5SBlock`: these are the two-channel variants of `BC4UBlock` and `BC4SBlock`. They simply contain a pair of their respective `BC4*Block` types, one for R and one for G.

The Block Encoders
===================

Block data is produced by the block encoders. Encoders exist only for the primitive block types. Composite blocks are encoded by combining the output of multiple primitive encoders.

Using the block encoders is easy.

Creating an Encoder
-------------------

Setting up a new encoder object is simple. Just create one and set its properties. Each encoder has just a handful of properties.

```cs
//create an encoder
var encBC1 = new BC1BlockEncoder();
//set its properties
encBC1.DitherRgb = true;
encBC1.DitherAlpha = true;
```

Encoding Data
-------------------

Encoding is a two-stage process. You begin by loading a block's worth of data into the encoder, and then finish by calling `Encode()`.

Data is loaded from arrays of floats. In order to accommodate as many storage layouts as possible, the encoders accept index and pitch values.

### BC1BlockEncoder

The most complex of the encoders is `BC1BlockEncoder` since it deals with multiple channels of data at once.

```cs
public void LoadBlock(
    float[] rValues, int rIndex,
    float[] gValues, int gIndex,
    float[] bValues, int bIndex,
    int rowPitch = 4, int colPitch = 1 );
```

This function loads a block of RGB data into the encoder. While it accepts three separate array parameters, there is no requirement that these be unique arrays. For instance, if your image data is in a single array of floats in RGBA order, then you'd load the block at coordinates 8, 12 as follows:

```cs
var blockX = 8;
var blockY = 12;

var blockOffset = (blockY * imageWidth + blockX) * 4;

encBC1.LoadBlock(
    imageData, blockOffset + 0,
    imageData, blockOffset + 1,
    imageData, blockOffset + 2,
    imageWidth * 4, 4 );
```

By default, the encoder will produce a solid block. Enabling alpha mode is as simple as loading an alpha mask using the similar `LoadAlphaMask` method. To reset to solid mode, you can either load a solid mask, or call `ClearAlphaMask`.

### BC2ABlockEncoder and BC4BlockEncoder

These encoders are far simpler. It only handles a single channel of data at a time and have a `LoadBlock` method that works a lot like `BC1BlockEncoder.LoadAlphaMask`.

Note that `BC4BlockEncoder` has *two* `Encode` variants, one for signed data and one for unsigned data.

The Image Encoders
===================

The library contains convenient wrappers around the block encoders for image-at-once encoding. These operate using two types: `FloatImage` is used for floating-point input data, and `BCnImage<T>` is used for output encoded data. The image types are very simple, they basically just wrap arrays of values.

Encoding an image at once is done using `ImageEncoder`. Using this type is very simple, just create one, set its (few) properties, and call the appropriate `Encode` method (one exists for each block type). If you have a multicore machine, `ImageEncoder` will automatically split the work across multiple threads to keep things speedy.