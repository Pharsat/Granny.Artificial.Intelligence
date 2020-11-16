import tensorflow as tf
from tensorflow import keras
import numpy as np
import io
from flask import Flask, request, jsonify
import pickle
from tensorflow.keras.preprocessing.text import Tokenizer
from tensorflow.keras.preprocessing.sequence import pad_sequences

app = Flask(__name__)


@app.route('/api/model/predict', methods=['POST'])
def predict_vector():
    data = request.get_json(force=True)
    sentence = data["sentence"]
    trunc_type = 'post'
    # loading
    with open('tokenizer.pickle', 'rb') as handle:
        tokenizer = pickle.load(handle)

    with open('max_length.pickle', 'rb') as handle:
        max_length = pickle.load(handle)

    sequences = tokenizer.texts_to_sequences(sentence)
    padded = pad_sequences(sequences, maxlen=max_length, truncating=trunc_type)

    model = keras.models.load_model('granny_ia_model.h5')
    model.summary()
    prediction = model.predict(padded)

    return str(prediction[0])


if __name__ == '__main__':
    try:
        app.run(port=8081, debug=True)
    except:
        print("Server is exited unexpectedly. Please contact server admin.")

