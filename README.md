# CamSliderPiControl
A Bluetooth-enabled RaspberryPi touch screen controller for Isaac879's 3-Axis camera slider.

A work-in-progress, started on January 1, 2021.  Isaac's brilliant 3-axis camera slider is great, and I wanted to add a portable controller option.  He provided a sample class file for an XBOX controller that you could run through a PC, but I wanted a more-portable and dedicated solution.

Isaac879's project is here: https://github.com/isaac879/Pan-Tilt-Mount

I wanted to add controller that was:
- battery-powered
- touchscreen-based
- Bluetooth-enabled
- visual with feedback of status/position
- knuckle-head simple

I wrote a similar controller in Microsoft UWP user interface for a robot owl, so I had fewer remaining puzzles to solve than most folks. You can see that project here: https://github.com/MrScottFletcher/BigOwl

STATUS: 2021-01-05: the master branch has the version that I demoed on YouTube https://youtu.be/O-RXUugMtgQ - It has it's quirks, but is a good start.  I did not implement the shutter or Timelapse functionality, and the Bluetooth comm manager is janky AF.  I'm done with this for a while.  In the meantime, I'm looking forward to seeing how you can improve it.  :)
