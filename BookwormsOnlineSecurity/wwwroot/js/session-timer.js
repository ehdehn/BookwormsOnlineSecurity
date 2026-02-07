let timeLeft = 30;
const countdown = document.getElementById('countdown');
let timerId = null;
let pingCooldown = false;

const resetTimer = () => {
    timeLeft = countdown?.dataset?.seconds ? parseInt(countdown.dataset.seconds, 10) : 30;
    if (countdown) countdown.textContent = timeLeft;
};

const sendPing = async () => {
    if (pingCooldown) return;
    pingCooldown = true;
    try {
        await fetch('/session/ping', { method: 'POST', headers: { 'X-Requested-With': 'XMLHttpRequest' } });
    } catch {
        // ignore network errors for ping
    } finally {
        // throttle pings to once every 5 seconds
        setTimeout(() => { pingCooldown = false; }, 5000);
    }
};

if (countdown) {
    resetTimer();

    timerId = setInterval(() => {
        timeLeft--;
        countdown.textContent = timeLeft;
        if (timeLeft <= 0) {
            clearInterval(timerId);
            window.location.href = '/Login?timeout=1';
        }
    }, 1000);

    const activityHandler = () => {
        resetTimer();
        sendPing();
    };

    ['mousemove', 'keydown', 'click', 'scroll', 'touchstart'].forEach(evt => {
        window.addEventListener(evt, activityHandler, { passive: true });
    });
}
