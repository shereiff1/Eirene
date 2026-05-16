import torch
from transformers import RobertaForSequenceClassification, RobertaTokenizer

# Load tokenizer
tokenizer = RobertaTokenizer.from_pretrained("roberta-base")

# Create model architecture
model = RobertaForSequenceClassification.from_pretrained(
    "roberta-base", num_labels=2  # change if your model has different labels
)

# Load weights
state_dict = torch.load("best_roberta_model_v1.pt", map_location=torch.device("cpu"))
model.load_state_dict(state_dict)

model.eval()

# Example test text
text = "I feel very stressed and tired lately"

# Tokenize
inputs = tokenizer(text, return_tensors="pt", truncation=True, padding=True)

# Run inference
with torch.no_grad():
    outputs = model(**inputs)

# Get prediction
logits = outputs.logits
prediction = torch.argmax(logits, dim=1)

print("Text:", text)
print("Prediction class:", prediction.item())
print("Raw logits:", logits)
