"""Konwerter JSONa z formatu C# ($id, $values) do czystego JSONa"""
import json

def clean_csharp_json(obj):
    """Usuwa $id, $values i $ref z JSONa C#"""
    if isinstance(obj, dict):
        # Jeśli ma $values, zwróć to
        if '$values' in obj:
            return clean_csharp_json(obj['$values'])
        
        # Usuń $id i $ref, przetworz resztę
        cleaned = {}
        for key, value in obj.items():
            if key not in ['$id', '$ref']:
                cleaned[key] = clean_csharp_json(value)
        return cleaned
    
    elif isinstance(obj, list):
        return [clean_csharp_json(item) for item in obj]
    
    else:
        return obj

def convert_teachers_json(input_file='teachers.json', output_file='teachers_clean.json'):
    """Konwertuje plik teachers.json do czystego formatu"""
    with open(input_file, 'r', encoding='utf-8') as f:
        data = json.load(f)
    
    cleaned = clean_csharp_json(data)
    
    with open(output_file, 'w', encoding='utf-8') as f:
        json.dump(cleaned, f, ensure_ascii=False, indent=2)
    
    print(f"✓ Skonwertowano {input_file} -> {output_file}")
    print(f"✓ Liczba nauczycieli: {len(cleaned)}")
    return cleaned

if __name__ == '__main__':
    convert_teachers_json()