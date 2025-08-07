document.getElementById('generateButton').addEventListener('click', async () => {
    const length = parseInt(document.getElementById('passwordLength').value);
    const includeUppercase = document.getElementById('includeUppercase').checked;
    const includeLowercase = document.getElementById('includeLowercase').checked;
    const includeDigits = document.getElementById('includeDigits').checked;
    const includeSpecialChars = document.getElementById('includeSpecialChars').checked;
    const generatedPasswordInput = document.getElementById('generatedPassword');
    const generationMessage = document.getElementById('generationMessage');

    generatedPasswordInput.value = '';
    generationMessage.style.display = 'none';
    generationMessage.textContent = '';

    if (length < 8 || length > 32) {
        generationMessage.textContent = 'Дължината на паролата трябва да е между 8 и 32 символа.';
        generationMessage.className = 'mt-2 text-danger';
        generationMessage.style.display = 'block';
        return;
    }

    if (!includeUppercase && !includeLowercase && !includeDigits && !includeSpecialChars) {
        generationMessage.textContent = 'Моля, изберете поне един тип символи.';
        generationMessage.className = 'mt-2 text-danger';
        generationMessage.style.display = 'block';
        return;
    }

    try {
        const response = await fetch('/api/RandomPasswordGenerator/generate', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json'
            },
            body: JSON.stringify({
                length: length,
                includeUppercase: includeUppercase,
                includeLowercase: includeLowercase,
                includeDigits: includeDigits,
                includeSpecialChars: includeSpecialChars
            })
        });

        if (response.ok) {
            const result = await response.json();
            generatedPasswordInput.value = result.generatedPassword;
            generationMessage.textContent = result.message || 'Паролата е генерирана успешно.';
            generationMessage.className = 'mt-2 text-success';
            generationMessage.style.display = 'block';
        } else {
            let errorMessage = `Грешка: ${response.status} ${response.statusText}`;
            try {
                const errorData = await response.json();
                errorMessage = errorData.errors ? Object.values(errorData.errors).flat().join('; ') : (errorData.message || errorData.title || errorMessage);
            } catch (jsonError) {
                errorMessage = `Грешка: ${response.status} ${response.statusText}. Невалиден отговор от сървъра.`;
                console.error('Failed to parse error response as JSON:', jsonError);
            }
            generationMessage.textContent = errorMessage;
            generationMessage.className = 'mt-2 text-danger';
            generationMessage.style.display = 'block';
        }
    } catch (error) {
        console.error('Fetch error:', error);
        generationMessage.textContent = `Възникна мрежова грешка: ${error.message}.`;
        generationMessage.className = 'mt-2 text-danger';
        generationMessage.style.display = 'block';
    }
});