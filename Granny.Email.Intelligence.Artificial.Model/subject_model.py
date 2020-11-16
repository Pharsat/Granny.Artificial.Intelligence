import tensorflow as tf
from tensorflow import keras
import numpy as np
import io
from flask import Flask, request, jsonify
import pickle
from tensorflow.keras.preprocessing.text import Tokenizer
from tensorflow.keras.preprocessing.sequence import pad_sequences

app = Flask(__name__)


@app.route('/api/model/generate', methods=['POST'])
def generate_model():
    # load data from post body data = request.get_json(force=True)

    training_sentences = ["hola soy bueno", "hola soy mejor", "hola soy positivo", "hola soy malo"]
    training_labels = [0, 0, 0, 10]

    testing_sentences = ["hola soy bueno", "hola soy malo"]
    testing_labels = [0, 10]

    training_labels_final = np.array(training_labels)
    testing_labels_final = np.array(testing_labels)

    vocab_size = 6 + 2
    embedding_dim = 16
    max_length = 10
    trunc_type = 'post'
    oov_tok = "<OOV>"

    tokenizer = Tokenizer(num_words=vocab_size, oov_token=oov_tok)
    tokenizer.fit_on_texts(training_sentences)
    # saving
    with open('tokenizer.pickle', 'wb') as handle:
        pickle.dump(tokenizer, handle, protocol=pickle.HIGHEST_PROTOCOL)

    with open('max_length.pickle', 'wb') as handle:
        pickle.dump(max_length, handle, protocol=pickle.HIGHEST_PROTOCOL)

    word_index = tokenizer.word_index
    sequences = tokenizer.texts_to_sequences(training_sentences)
    padded = pad_sequences(sequences, maxlen=max_length, truncating=trunc_type)

    testing_sequences = tokenizer.texts_to_sequences(testing_sentences)
    testing_padded = pad_sequences(testing_sequences, maxlen=max_length)

    reverse_word_index = dict([(value, key) for (key, value) in word_index.items()])

    model = tf.keras.Sequential([
        tf.keras.layers.Embedding(vocab_size, embedding_dim, input_length=max_length),
        tf.keras.layers.GlobalAveragePooling1D(),
        tf.keras.layers.Dense(6, activation='relu'),
        tf.keras.layers.Dense(1, activation='sigmoid')
    ])

    model.compile(loss='binary_crossentropy', optimizer='adam', metrics=['accuracy'])
    model.summary()

    num_epochs = 10
    model.fit(padded, training_labels_final, epochs=num_epochs, validation_data=(testing_padded, testing_labels_final))

    model.save('granny_ia_model.h5')

    e = model.layers[0]
    weights = e.get_weights()[0]

    out_v = io.open('vecs.tsv', 'w', encoding='utf-8')
    out_m = io.open('meta.tsv', 'w', encoding='utf-8')
    for word_num in range(1, vocab_size):
        word = reverse_word_index[word_num]
        embeddings = weights[word_num]
        out_m.write(word + "\n")
        out_v.write('\t'.join([str(x) for x in embeddings]) + "\n")
    out_v.close()
    out_m.close()

    return jsonify('ok')


@app.route('/api/model/predict', methods=['POST'])
def predict_vector():
    # load data from post body data = request.get_json(force=True)

    trunc_type = 'post'

    example = ["hola soy malo"]
    # loading
    with open('tokenizer.pickle', 'rb') as handle:
        tokenizer = pickle.load(handle)

    with open('max_length.pickle', 'rb') as handle:
        max_length = pickle.load(handle)

    sequences = tokenizer.texts_to_sequences(example)
    padded = pad_sequences(sequences, maxlen=max_length, truncating=trunc_type)

    model = keras.models.load_model('granny_ia_model.h5')
    model.summary()
    prediction = model.predict(padded)

    return str(prediction[0])


if __name__ == '__main__':
    try:
        app.run(port=5000, debug=True)
    except:
        print("Server is exited unexpectedly. Please contact server admin.")
