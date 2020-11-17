class TrainRequest:
    def __init__(self, training_sentences, training_labels, testing_sentences, testing_labels, total_words, max_length):
        self.training_sentences = training_sentences
        self.training_labels = training_labels
        self.testing_sentences = testing_sentences
        self.testing_labels = testing_labels
        self.total_words = total_words
        self.max_length = max_length