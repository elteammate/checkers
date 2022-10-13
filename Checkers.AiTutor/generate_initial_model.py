import json

import numpy as np
import tensorflow as tf
from tensorflow import keras

inputs = keras.Input(shape=(32,), name="board")
hidden = [
    keras.layers.Dense(60, activation="tanh", name="hidden_1"),
    keras.layers.Dense(10, activation="tanh", name="hidden_2"),
]
outputs = keras.layers.Dense(1, activation="tanh", name="output")

model = keras.Sequential([inputs] + hidden + [outputs], name="checkers")
model.compile(optimizer="adam", loss="mse", metrics=["accuracy"])

samples = open("data/samples.txt").read().splitlines()
samples = np.array([list(map(lambda x: float(x.replace(",", ".")), sample.split())) for sample in samples])

train_input_size = int(len(samples) * 0.8)
test_input_size = len(samples) - train_input_size

train_inputs = samples[:train_input_size, :32]
train_outputs = samples[:train_input_size, 32:]

test_inputs = samples[train_input_size:, :32]
test_outputs = samples[train_input_size:, 32:]

model.fit(train_inputs, train_outputs, epochs=10, batch_size=128)

scores = model.evaluate(test_inputs, test_outputs, batch_size=128)
print("Test loss:", scores[0])
print("Test accuracy:", scores[1])

json_model = {
    "K": 2,
    "Weights": [
        layer.get_weights()[0].tolist() + [layer.get_weights()[1].tolist()] for layer in model.layers
    ]
}

json.dump(json_model, open("data/model.json", "w"))

for i in range(300):
    json.dump(json_model, open(f"data/current/{i}.json", "w"))
