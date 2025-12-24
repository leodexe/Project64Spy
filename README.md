Project64Spy with support for Kaillera versions of Project64, and other emulators.
======

Tested emulators: Project64 1.4, 1.6, 1.7, 2.3.x, 2.4, 3.0.1, AQZ Netplay, 4.0.x

This fork adds support to other versions of Project64 with Kaillera Netplay, specifically:
- Project64k - Father of all Kaillera forks below
- Project64KVE - Kaillera (Vista)
- Project64K7E - Kaillera (Windows 7)
- Project64KSE - Kaillera (Smash Edition)
- Project64SE - Standard Edition

- Mupen64 - specifically versions 1.0.9, 1.0.9.1, 1.0.10 and legacy versions already support:
- Mupen64-rerecording
- Mupen64-pucrash
- Mupen64_lua
- Mupen64-wiivc
- Mupen64-RTZ
- Mupen64-rrv8-avisplit
- Mupen64-rerecording-v2-reset

- RetroArch (already supported)
- Wine Preloader (already supported)

Now you can use Project64Spy without having to rename each emulator instance, this aims to fix an issue where Project64Spy and autosplitters couldn't be used at the same time, since autosplitters requires either Project64KVE or Project64KSE to work. 

# Compatibility List
- TODO: Test the Spy with all games for a detailed compatibility list.
- Mupen64 support is limited to the list above, so it doesn't support Mupen64k, Mupen64plus or the frontend M64py. Send a PR if you figure out a solution for this.

## How to compile Project64Spy from source
- Download Visual Studio 2022 or 2026, any of them will do.
- Unzip the source, open Project64Spy.sln, there are two projects inside, MIPSInterpreter and Project64Spy, you only need to compile the Project64Spy project.
- For consistency, you should always compile with these settings:
  - Configuration: Release, you should not use Debug, unless you're actively debugging.
  - Target Framework: 4.8 or 4.8.1, Output Type: Windows Application.
   - If Net Framework 4.8 and 4.8.1 don't appear in the Target Framework list, you have to download one of them, then restart Visual Studio, now you should be able to select them.
  - Target Platform: x64
  - Auto-generate binding redirects, Allow unsafe code, Optimize Code: All Enabled.
  - In the Solution Explorer, right click on Project64Spy project, select Compile. It should create "Project64Spy.exe in the bin folder. If it doesn't compile and shows some error message, then you likely did something wrong, remember to select Configuration: "Release" and Platform "x64 in the VS toolbar above.
 
## Creating your own skins

Each skin consists of a subfolder in the "skins" directory, which is expected to contain a file called ``skin.xml`` along with all the PNG image assets required by the skin.  The easiest way to create a skin for your target console is probably just to copy+paste the default skin and modify it according to your needs.  What follows is a thorough documentation of the skin.xml format for reference if you'd like to create more complex skins.

The root node of the ``skin.xml`` file must define the following 3 attributes:
```
<skin
    name="Default PC 360" # This is the name of the skin as it will appear in the selection list.
    author="jaburns"      # Your name or handle.
    type="pc360">         # The input type this skin is used for.
```
Valid values for the ``type`` attribute are as follows: ``nes``, ``snes``, ``n64``, ``gamecube``, ``pc360``, ``generic``. 

Each skin must define at least one ``<background>`` element.  Each background entry will be listed in the skin selector as a separate entry.  Every background for your skin must have the same dimensions.
```
<background
    name="Default"     # The name which will appear in the selection list.
    image="pad.png"    # Optional PNG file to use for this background selection.
    color="red"        # Optional Color to use as background, either a text or a hex (#123DEF) style
    /> 
```
The rest of the ``skin.xml`` file defines how to render button and analog inputs.  Each type exposes a variety of buttons and analog values.  Button inputs are mapped to images at specific locations using the ``<button>`` tag, and there is a small variety of possible ways to map analog inputs.

```
<detail
    name="Static Image"                  # Name of the source input button to map this image to.
    image="Dropshadow.png"               # Image file to display when this input is pressed.
    x="101" y="71"                       # Location in pixels where the top/left corner of the image should sit.
    width="16"                           # Width and height specification can OPTIONALLY be used to scale
    height="16" 
    target="Background A;Background C"   # Optional. Only show for this background. Available for all elements.
    ignore="Background B"                # Optional. Do not show for this background (not required when target is defined)
    />     #   an image to a specific size.  The default size is the orignal image size.
```
A static image to show, works great with ignore and targets to add more variety with ease to skins.

```
<button
    name="up"          # Name of the source input button to map this image to.
    image="circle.png" # Image file to display when this input is pressed.
    x="101" y="71"     # Location in pixels where the top/left corner of the image should sit.
    width="16"         # Width and height specification can OPTIONALLY be used to scale
    height="16" />     #   an image to a specific size.  The default size is the orignal image size.
```
Analog values can be mapping as sticks, ranges, or range-based buttons.  Ranges are used for things like analog shoulder buttons and render by filling an image by the amount the range is pressed.  Range buttons are useful for creating a button-like display when an analog value is in a certain ranage.  An example of this use case is if you are streaming a SNES game, but playing using a 360 controller, you can bind the analog stick to appear as if you are pressing the d-pad buttons.
```
<stick
    xname="lstick_x"  # Analogs values to bind the image's x 
    yname="lstick_y"  #   and y displacements to.
    image="stick.png" # Image file to use for the stick.
    x="53" y="31"     # Location in pixels where the top/left corner of the image should sit. 
    width="34"        # Width and height specification can OPTIONALLY be used to scale 
    height="35"       #   an image to a specific size.  The default size is the orignal image size.
    xrange="9"        # xrange and yrange specify how much to move the stick image in either axis
    yrange="9"        #   when the stick is deplaced.
    xreverse="false"  # Settings xreverse or yreverse to true will reverse the direction
    yreverse="true"   #   that the stick moves along that axis
    />     
```
```
<analog
    name="trig_l"      # Analog value to bind the display to.
    image="trig-l.png" # Image file to mask over the background as the input changes.
    x="15" y="18"      # Location in pixels where the top/left corner of the image should sit. 
    direction="up"     # The direction to sweep the image.
                         Valid options are 'up', 'down', 'left', 'right'.
    reverse="true"     # 'true' or 'false'. Setting to true will cause the image to clear instead
                         of fill as the analog value is further engaged.
    usenegative="true" # 'true' or 'false'. If set to true, then the image will change when the analog
                         value ranges from 0 to -1 instead of 0 to 1.  Useful for generic gamepad skins
                         who use a single axis for analog L/R buttons.
    />
```
```
<rangebutton
    name="lstick_x"    # Analog value to bind the display to.
    image="d-left.png" # Image file to mask over the background as the input changes.
    x="15" y="18"      # Location in pixels where the top/left corner of the image should sit. 
    from="-1.0"        # From and to attributes specify the range which the specified analog
    to="-0.5"          #   input must be in to display the image.
/>
```

### Binding controller inputs to keyboard key presses

Binding controller inputs to keyboard key presses is achieved by placing ``<binding>`` definitions
in the ``keybindings.xml`` file.  The ``output-key`` attribute on the binding specifies which keyboard key to press when the provided gamepad buttons are pressed.  Values of ``output-key`` can simply be letters, or [see here other for valid key bindings (the text in red are the acceptable values for ``output-key``)](https://github.com/jaburns/NintendoSpy/blob/master/Keybindings.cs#L110).  Each binding must contain at least one child ``<input>`` element.  The input elements specify which buttons on the controller must be depressed in order to send the key press signal.  See below for an example ``keybindings.xml`` file which makes pressing L and R together trigger a ``home`` key press on the keyboard.

```xml
<?xml version="1.0" encoding="UTF-8"?>
<keybindings>
    <binding output-key="home">
        <input button="r" />
        <input button="l" />
    </binding>
</keybindings>
