import os
import torch
from transformers import RobertaForSequenceClassification, RobertaTokenizer

class ModelPredictor:
    def __init__(self, model_path="best_roberta_model_v1.pt"):
        self.device = torch.device("cpu")
        self.tokenizer = RobertaTokenizer.from_pretrained("roberta-base")
        self.model = RobertaForSequenceClassification.from_pretrained(
            "roberta-base", num_labels=2
        )
        if os.path.exists(model_path):
            state_dict = torch.load(model_path, map_location=self.device, weights_only=True)
            self.model.load_state_dict(state_dict)
        else:
            print(f"Warning: Model file {model_path} not found. Using untrained weights.")
        self.model.to(self.device)
        self.model.eval()

    def predict(self, text):
        inputs = self.tokenizer(text, return_tensors="pt", truncation=True, padding=True).to(self.device)
        with torch.no_grad():
            outputs = self.model(**inputs)

        logits = outputs.logits
        prediction = torch.argmax(logits, dim=1).item()

        # prediction: 1 (issue detected) or 0 (no issue detected)
        return prediction

# Test block
if __name__ == "__main__":
    predictor = ModelPredictor()
    result = predictor.predict("I feel very stressed and tired lately")
    print(f"Prediction: {result}")
