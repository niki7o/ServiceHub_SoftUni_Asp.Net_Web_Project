document.addEventListener('DOMContentLoaded', function () {
    const dynamicTextElement = document.querySelector('.hero-text-dynamic');
    const texts = [
        "Генерирайте CV-та и договори.",
        "Конвертирайте файлове лесно.",
        "Бройте символи и думи.",
        "Създавайте сигурни пароли.",
        "Анализирайте кодови фрагменти."
    ];
    let textIndex = 0;
    let charIndex = 0;
    let isDeleting = false;
    let typingSpeed = 100;
    let delayBeforeDelete = 1500;
    let delayBeforeType = 500;

    function typeWriter() {
        const currentText = texts[textIndex];
        if (isDeleting) {
            dynamicTextElement.textContent = currentText.substring(0, charIndex--);
        } else {
            dynamicTextElement.textContent = currentText.substring(0, charIndex++);
        }

        let speed = typingSpeed;
        if (isDeleting) {
            speed /= 2;
        }

        if (!isDeleting && charIndex === currentText.length + 1) {
            speed = delayBeforeDelete;
            isDeleting = true;
        } else if (isDeleting && charIndex === 0) {
            isDeleting = false;
            textIndex = (textIndex + 1) % texts.length;
            speed = delayBeforeType;
        }

        setTimeout(typeWriter, speed);
    }

    typeWriter();
});