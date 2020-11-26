import io
import pickle
import numpy as np
import tensorflow as tf
from flask import Flask, jsonify, request
from tensorflow.keras.preprocessing.sequence import pad_sequences
from tensorflow.keras.preprocessing.text import Tokenizer
from train_request import TrainRequest

app = Flask(__name__)


def data_to_train_request(data):
    training_sentences = data["training_sentences"]
    training_labels = data["training_labels"]

    testing_sentences = data["test_sentences"]
    testing_labels = data["test_labels"]

    total_words = data["total_words"]
    max_length = data["sentence_max_lenght"]

    training_labels_final = np.array(training_labels)
    testing_labels_final = np.array(testing_labels)
    return TrainRequest(training_sentences, training_labels_final, testing_sentences, testing_labels_final, total_words,
                        max_length)


def save_tsv(model, vocab_size, reverse_word_index, prefix):
    e = model.layers[0]
    weights = e.get_weights()[0]

    out_v = io.open(f'{prefix}vecs.tsv', 'w', encoding='utf-8')
    out_m = io.open(f'{prefix}meta.tsv', 'w', encoding='utf-8')
    for word_num in range(1, vocab_size):
        word = reverse_word_index[word_num]
        embeddings = weights[word_num]
        out_m.write(word + "\n")
        out_v.write('\t'.join([str(x) for x in embeddings]) + "\n")
    out_v.close()
    out_m.close()


def model_train(data, prefix):
    train_request = data_to_train_request(data)
    vocab_size = train_request.total_words + 2
    embedding_dim = 16
    trunc_type = 'post'
    oov_tok = "<OOV>"

    tokenizer = Tokenizer(num_words=vocab_size, oov_token=oov_tok)
    tokenizer.fit_on_texts(train_request.training_sentences)
    # saving
    with open(f'{prefix}tokenizer.pickle', 'wb') as handle:
        pickle.dump(tokenizer, handle, protocol=pickle.HIGHEST_PROTOCOL)

    with open(f'{prefix}max_length.pickle', 'wb') as handle:
        pickle.dump(train_request.max_length, handle, protocol=pickle.HIGHEST_PROTOCOL)

    word_index = tokenizer.word_index
    sequences = tokenizer.texts_to_sequences(train_request.training_sentences)
    padded = pad_sequences(sequences, maxlen=train_request.max_length, truncating=trunc_type)

    testing_sequences = tokenizer.texts_to_sequences(train_request.testing_sentences)
    testing_padded = pad_sequences(testing_sequences, maxlen=train_request.max_length)

    model = tf.keras.Sequential([
        tf.keras.layers.Embedding(vocab_size, embedding_dim, input_length=train_request.max_length),
        #tf.keras.layers.GlobalAveragePooling1D(),
        #tf.keras.layers.Flatten(),
        tf.keras.layers.Bidirectional(tf.keras.layers.LSTM(train_request.max_length)),
        tf.keras.layers.Dense(6, activation='relu'),
        tf.keras.layers.Dense(1, activation='sigmoid')
    ])

    model.compile(loss='binary_crossentropy', optimizer='adam', metrics=['accuracy'])
    model.summary()

    num_epochs = 10
    model.fit(padded, train_request.training_labels, epochs=num_epochs,
              validation_data=(testing_padded, train_request.testing_labels))
    model.save(f'{prefix}model.h5')

    reverse_word_index = dict([(value, key) for (key, value) in word_index.items()])
    save_tsv(model, vocab_size, reverse_word_index, prefix)


@app.route('/api/model/train/header', methods=['POST'])
def generate_header_model():
    data = request.get_json(force=True)
    model_train(data, 'header')
    return jsonify('ok')


@app.route('/api/model/train/body', methods=['POST'])
def generate_body_model():
    data = request.get_json(force=True)
    model_train(data, 'body')
    return jsonify('ok')


@app.route('/api/model/train/subject', methods=['POST'])
def generate_subject_model():
    data = request.get_json(force=True)
    model_train(data, 'subject')
    return jsonify('ok')


if __name__ == '__main__':
    try:
        app.run(port=8082, debug=True)
    except:
        print("Server is exited unexpectedly. Please contact server admin.")
