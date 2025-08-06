document.getElementById('countButton').addEventListener('click', async () => {
    const inputText = document.getElementById('inputText').value;

    const wordCountSpan = document.getElementById('wordCount');
    const charCountSpan = document.getElementById('charCount');
    const lineCountSpan = document.getElementById('lineCount');
    const messageArea = document.getElementById('messageArea');


    wordCountSpan.textContent = '0';
    charCountSpan.textContent = '0';
    lineCountSpan.textContent = '0';
    messageArea.innerHTML = '';

    if (!inputText.trim()) {
        messageArea.innerHTML = '<div class="alert alert-warning">Моля, въведете текст за преброяване.</div>';
        return;
    }

    try {

        const response = await fetch('/api/WordCharacter/count', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json'
            },
            body: JSON.stringify({ text: inputText })
        });

        if (response.ok) {
            const result = await response.json();
            wordCountSpan.textContent = result.wordCount;
            charCountSpan.textContent = result.charCount;
            lineCountSpan.textContent = result.lineCount;
            if (result.message) {
                messageArea.innerHTML = `<div class="alert alert-success">${result.message}</div>`;
            }
        } else {

            let errorMessage = `Грешка: ${response.status} ${response.statusText}`;
            try {
                const errorData = await response.json();

                errorMessage = errorData.errors
                    ? Object.values(errorData.errors).flat().map(e => e.errorMessage || e).join('; ')
                    : (errorData.message || errorData.title || errorMessage);
            } catch (jsonError) {

                errorMessage = `Грешка: ${response.status} ${response.statusText}. Невалиден отговор от сървъра.`;
                console.error('Failed to parse error response as JSON:', jsonError);
            }
            messageArea.innerHTML = `<div class="alert alert-danger">${errorMessage}</div>`;
        }
    } catch (error) {

        console.error('Fetch error:', error);
        messageArea.innerHTML = `<div class="alert alert-danger">Възникна мрежова грешка: ${error.message}. Моля, проверете дали сървърът работи.</div>`;
    }
});