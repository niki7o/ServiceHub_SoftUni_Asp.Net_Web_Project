document.getElementById('convertButton').addEventListener('click', async () => {
    const inputText = document.getElementById('inputText').value;
    const caseType = document.getElementById('caseTypeSelect').value;
    const outputResult = document.getElementById('outputResult');
    const conversionMessage = document.getElementById('conversionMessage');

    outputResult.value = '';
    conversionMessage.style.display = 'none';
    conversionMessage.textContent = '';

    if (!inputText.trim()) {
        conversionMessage.textContent = 'Моля, въведете текст.';
        conversionMessage.className = 'mt-2 text-danger';
        conversionMessage.style.display = 'block';
        return;
    }

    try {
        const response = await fetch('/api/TextCaseConverter/convert', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json'
            },
            body: JSON.stringify({ text: inputText, caseType: caseType })
        });

        if (response.ok) {
            const result = await response.json();
            outputResult.value = result.convertedText;
            if (result.message) {
                conversionMessage.textContent = result.message;
                conversionMessage.className = 'mt-2 text-success';
                conversionMessage.style.display = 'block';
            }
        } else {
            let errorMessage = `Грешка: ${response.status} ${response.statusText}`;
            try {
                const errorData = await response.json();
                errorMessage = errorData.errors ? Object.values(errorData.errors).flat().join('; ') : (errorData.message || errorData.title || errorMessage);
            } catch (jsonError) {
                errorMessage = `Грешка: ${response.status} ${response.statusText}. Невалиден отговор от сървъра.`;
                console.error('Failed to parse error response as JSON:', jsonError);
            }
            outputResult.value = '';
            conversionMessage.textContent = errorMessage;
            conversionMessage.className = 'mt-2 text-danger';
            conversionMessage.style.display = 'block';
        }
    } catch (error) {
        console.error('Fetch error:', error);
        outputResult.value = '';
        conversionMessage.textContent = `Възникна мрежова грешка: ${error.message}.`;
        conversionMessage.className = 'mt-2 text-danger';
        conversionMessage.style.display = 'block';
    }
});