import pickle

from flask import Flask, request
from tensorflow import keras
from tensorflow.keras.preprocessing.sequence import pad_sequences

from utils import Suffixes, Prefixes

app = Flask(__name__)


def predict(data, prefix):
    sentence = data["sentence"]
    trunc_type = 'post'
    # loading
    with open(f'{prefix}{Suffixes.tokenizer}', 'rb') as handle:
        tokenizer = pickle.load(handle)

    with open(f'{prefix}{Suffixes.max_length}', 'rb') as handle:
        max_length = pickle.load(handle)

    sequences = tokenizer.texts_to_sequences(sentence)
    padded = pad_sequences(sequences, maxlen=max_length, truncating=trunc_type)

    model = keras.models.load_model(f'{prefix}{Suffixes.model}')
    model.summary()
    prediction = model.predict(padded)

    return str(prediction[0])


@app.route('/api/model/predict/body', methods=['POST'])
def predict_vector_body():
    data = request.get_json(force=True)
    prediction = predict(data, Prefixes.body)
    return prediction


@app.route('/api/model/predict/header', methods=['POST'])
def predict_vector_header():
    data = request.get_json(force=True)
    prediction = predict(data, Prefixes.header)
    return prediction


@app.route('/api/model/predict/subject', methods=['POST'])
def predict_vector_subject():
    data = request.get_json(force=True)
    prediction = predict(data, Prefixes.subject)
    return prediction


if __name__ == '__main__':
    try:
        app.run(port=8081, debug=True)
    except:
        print("Server is exited unexpectedly. Please contact server admin.")
