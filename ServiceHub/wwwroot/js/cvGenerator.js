document.getElementById('cvGeneratorForm').addEventListener('submit', async function (event) {
    event.preventDefault();

    const form = event.target;
    const messageArea = document.getElementById('messageArea');

    messageArea.innerHTML = '';
    document.getElementById('nameError').textContent = '';
    document.getElementById('emailError').textContent = '';
    document.getElementById('phoneError').textContent = '';
    document.getElementById('summaryError').textContent = '';
    document.getElementById('educationError').textContent = '';
    document.getElementById('experienceError').textContent = '';
    document.getElementById('skillsError').textContent = '';

    messageArea.innerHTML = '<div class="alert alert-info"><i class="fas fa-spinner fa-spin me-2"></i>Генерирането на CV е в ход... Моля, изчакайте.</div>';

    const requestData = {
        name: document.getElementById('name').value,
        email: document.getElementById('email').value,
        phone: document.getElementById('phone').value,
        summary: document.getElementById('summary').value,
        education: document.getElementById('education').value,
        experience: document.getElementById('experience').value,
        skills: document.getElementById('skills').value
    };

    let isValid = true;
    if (requestData.name.length > 100) {
        document.getElementById('nameError').textContent = 'Името не може да надвишава 100 символа.';
        isValid = false;
    }
    if (requestData.email.length > 100) {
        document.getElementById('emailError').textContent = 'Имейлът не може да надвишава 100 символа.';
        isValid = false;
    }
    if (requestData.phone.length > 50) {
        document.getElementById('phoneError').textContent = 'Телефонният номер не може да надвишава 50 символа.';
        isValid = false;
    }
    if (requestData.summary.length > 2000) {
        document.getElementById('summaryError').textContent = 'Резюмето не може да надвишава 2000 символа.';
        isValid = false;
    }
    if (requestData.education.length > 2000) {
        document.getElementById('educationError').textContent = 'Образованието не може да надвишава 2000 символа.';
        isValid = false;
    }
    if (requestData.experience.length > 4000) {
        document.getElementById('experienceError').textContent = 'Опитът не може да надвишава 4000 символа.';
        isValid = false;
    }
    if (requestData.skills.length > 2000) {
        document.getElementById('skillsError').textContent = 'Уменията не могат да надвишават 2000 символа.';
        isValid = false;
    }

    const emailRegex = /^[^\s@@]+@@[^\s@@]+\.[^\s@@]+$/;
    if (requestData.email && !emailRegex.test(requestData.email)) {
        document.getElementById('emailError').textContent = 'Невалиден формат на имейл адрес.';
        isValid = false;
    }

    if (!requestData.name.trim()) {
        document.getElementById('nameError').textContent = 'Името е задължително.';
        isValid = false;
    }
    if (!requestData.email.trim()) {
        document.getElementById('emailError').textContent = 'Имейлът е задължителен.';
        isValid = false;
    }

    if (!isValid) {
        messageArea.innerHTML = '<div class="alert alert-danger">Моля, коригирайте грешките във формата.</div>';
        return;
    }

    try {
        const response = await fetch('/api/CvGenerator/generate', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json'
            },
            body: JSON.stringify(requestData)
        });

        if (response.ok) {
            const blob = await response.blob();
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;

            const contentDisposition = response.headers.get('Content-Disposition');
            let filename = "generated_cv.pdf";
            if (contentDisposition && contentDisposition.indexOf('filename=') !== -1) {
                const filenameMatch = /filename\*?=['"]?(?:UTF-8'')?([^;"]+)/i.exec(contentDisposition);
                if (filenameMatch && filenameMatch[1]) {
                    filename = decodeURIComponent(filenameMatch[1].replace(/\+/g, ' '));
                }
            }

            a.download = filename;
            document.body.appendChild(a);
            a.click();
            window.URL.revokeObjectURL(url);
            a.remove();

            messageArea.innerHTML = `<div class="alert alert-success"><i class="fas fa-check-circle me-2"></i>CV файлът '${filename}' е успешно генериран и изтеглен!</div>`;
        } else {
            let errorText = await response.text();
            try {
                const errorJson = JSON.parse(errorText);
                if (errorJson.errors) {
                    if (errorJson.errors.Name) {
                        document.getElementById('nameError').textContent = errorJson.errors.Name.join('; ');
                    }
                    if (errorJson.errors.Email) {
                        document.getElementById('emailError').textContent = errorJson.errors.Email.join('; ');
                    }
                    if (errorJson.errors.Phone) {
                        document.getElementById('phoneError').textContent = errorJson.errors.Phone.join('; ');
                    }
                    if (errorJson.errors.Summary) {
                        document.getElementById('summaryError').textContent = errorJson.errors.Summary.join('; ');
                    }
                    if (errorJson.errors.Education) {
                        document.getElementById('educationError').textContent = errorJson.errors.Education.join('; ');
                    }
                    if (errorJson.errors.Experience) {
                        document.getElementById('experienceError').textContent = errorJson.errors.Experience.join('; ');
                    }
                    if (errorJson.errors.Skills) {
                        document.getElementById('skillsError').textContent = errorJson.errors.Skills.join('; ');
                    }
                }
                errorText = errorJson.message || response.statusText;
            } catch (e) {
                console.error("Failed to parse error response as JSON:", e);
            }
            messageArea.innerHTML = `<div class="alert alert-danger"><i class="fas fa-times-circle me-2"></i>Грешка при генериране на CV: ${errorText}</div>`;
        }
    } catch (error) {
        console.error('Fetch error:', error);
        messageArea.innerHTML = `<div class="alert alert-danger"><i class="fas fa-exclamation-triangle me-2"></i>Възникна мрежова грешка: ${error.message}.</div>`;
    }
});