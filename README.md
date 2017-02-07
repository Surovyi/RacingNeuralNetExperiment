# RacingNeuralNetExperiment
My first step into neural networks. Will expand this project using new techniques and practices.

Firstly, sorry for my englando.

The main goal of this project is to understand how neural networks work, its history and additional ways to improve it.
Currently I made classical neural network that uses genetic algorithm to learn. In the same time, I'm reading information
about different types of evolution that would likely fit conditions of this project.

I like making videogames so I'm using Unity 5.5 as well as C# to be able to watch results of my work.
The goal of the neural network is to teach a car to race without dying. If car collides with a wall, it means death for it. Currently project contains three tracks and this is not the end. First one is the most simple and the third one is the most difficult.

Next goal would be to make car smarter, thus it will ride without dragging, faster, find secret paths (planning to add one
into the 3rd track), and compete with other cars.

Car has 9 sensors. 8 of them are a linecasts for different directions and the last one is the current speed. These sensors are 
input neurons to neural net. Number of hidden layers and neurons may vary manually (currently I'm using 2 hidden layers with 30 neurons for each), but this will be changed later. At the output neural network has two neurons which are used to drive a car.

Car was taken from Unity Standart Assets and contains all it's functionality except that neural network trying to simulate pressing the
WASD keys instead of user input system.
