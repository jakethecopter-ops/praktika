<script setup>
import { computed, onMounted, ref } from 'vue'
import axios from 'axios'

const api = axios.create({
    baseURL: 'http://localhost:5102/api'
})

const activeTab = ref('programs')
const errorMessage = ref('')

const programs = ref([])
const exercises = ref([])
const activities = ref([])
const summary = ref(null)

function today() {
    return new Date().toISOString().slice(0, 10)
}

const newProgram = ref({ name: '', type: '', isActive: true })
const newExercise = ref({ name: '', programId: '', isActive: true })
const newActivity = ref({
    date: today(),
    durationMinutes: 30,
    notes: '',
    exerciseId: ''
})
const summaryDate = ref(today())

const activeExercises = computed(() =>
    exercises.value.filter((exercise) => exercise.isActive)
)

const stickerClass = computed(() => {
    if (!summary.value) {
        return ''
    }
    return summary.value.stickerColor.toLowerCase()
})

async function loadPrograms() {
    const response = await api.get('/Programs')
    programs.value = response.data
}

async function loadExercises() {
    const response = await api.get('/Exercises')
    exercises.value = response.data
}

async function loadActivities() {
    const response = await api.get('/Activities')
    activities.value = response.data
}

async function loadSummary() {
    const response = await api.get('/Activities/daily-summary', {
        params: { date: summaryDate.value }
    })
    summary.value = response.data
}

async function loadAll() {
    await loadPrograms()
    await loadExercises()
    await loadActivities()
    await loadSummary()
}

async function addProgram() {
    await api.post('/Programs', newProgram.value)
    newProgram.value = { name: '', type: '', isActive: true }
    await loadPrograms()
}

async function addExercise() {
    await api.post('/Exercises', newExercise.value)
    newExercise.value = { name: '', programId: '', isActive: true }
    await loadExercises()
}

async function addActivity() {
    await api.post('/Activities', newActivity.value)
    newActivity.value = { date: today(), durationMinutes: 30, notes: '', exerciseId: '' }
    await loadActivities()
    await loadSummary()
}

async function removeProgram(id) {
    await api.delete(`/Programs/${id}`)
    await loadPrograms()
}

async function removeExercise(id) {
    await api.delete(`/Exercises/${id}`)
    await loadExercises()
}

async function removeActivity(id) {
    await api.delete(`/Activities/${id}`)
    await loadActivities()
    await loadSummary()
}

async function run(action) {
    errorMessage.value = ''
    try {
        await action()
    } catch (error) {
        const data = error.response?.data
        errorMessage.value = typeof data === 'string'
            ? data
            : data?.title ?? error.message
    }
}

onMounted(loadAll)
</script>

<template>
    <div class="page">
        <h1>Учёт тренировок</h1>
        <p v-if="errorMessage" class="error">{{ errorMessage }}</p>

        <nav class="tabs">
            <button :class="{ active: activeTab === 'programs' }" @click="activeTab = 'programs'">
                Программы
            </button>
            <button :class="{ active: activeTab === 'exercises' }" @click="activeTab = 'exercises'">
                Упражнения
            </button>
            <button :class="{ active: activeTab === 'activities' }" @click="activeTab = 'activities'">
                Активности
            </button>
        </nav>

        <section v-if="activeTab === 'programs'">
            <h2>Тренировочные программы</h2>
            <form class="row" @submit.prevent="run(addProgram)">
                <input v-model="newProgram.name" placeholder="Название" required />
                <input v-model="newProgram.type" placeholder="Тип" required />
                <label>
                    <input type="checkbox" v-model="newProgram.isActive" />
                    Активная
                </label>
                <button type="submit">Добавить</button>
            </form>
            <ul>
                <li v-for="program in programs" :key="program.id">
                    {{ program.name }} ({{ program.type }}) —
                    {{ program.isActive ? 'активная' : 'неактивная' }}
                    <button class="danger" @click="run(() => removeProgram(program.id))">
                        Удалить
                    </button>
                </li>
            </ul>
        </section>

        <section v-else-if="activeTab === 'exercises'">
            <h2>Упражнения</h2>
            <form class="row" @submit.prevent="run(addExercise)">
                <input v-model="newExercise.name" placeholder="Название" required />
                <select v-model="newExercise.programId" required>
                    <option value="" disabled>Программа</option>
                    <option v-for="program in programs" :key="program.id" :value="program.id">
                        {{ program.name }}
                    </option>
                </select>
                <label>
                    <input type="checkbox" v-model="newExercise.isActive" />
                    Активное
                </label>
                <button type="submit">Добавить</button>
            </form>
            <ul>
                <li v-for="exercise in exercises" :key="exercise.id">
                    {{ exercise.name }} —
                    программа: {{ exercise.program?.name ?? '—' }} —
                    {{ exercise.isActive ? 'активное' : 'неактивное' }}
                    <button class="danger" @click="run(() => removeExercise(exercise.id))">
                        Удалить
                    </button>
                </li>
            </ul>
        </section>

        <section v-else>
            <h2>Активности</h2>
            <form class="row" @submit.prevent="run(addActivity)">
                <input type="date" v-model="newActivity.date" required />
                <input
                    type="number"
                    min="1"
                    max="1440"
                    v-model="newActivity.durationMinutes"
                    required
                />
                <input v-model="newActivity.notes" placeholder="Примечание" />
                <select v-model="newActivity.exerciseId" required>
                    <option value="" disabled>Упражнение</option>
                    <option v-for="exercise in activeExercises" :key="exercise.id" :value="exercise.id">
                        {{ exercise.name }}
                    </option>
                </select>
                <button type="submit">Добавить</button>
            </form>
            <ul>
                <li v-for="activity in activities" :key="activity.id">
                    {{ activity.date }} — {{ activity.durationMinutes }} мин —
                    {{ activity.exercise?.name ?? '—' }} —
                    {{ activity.notes || 'без примечания' }}
                    <button class="danger" @click="run(() => removeActivity(activity.id))">
                        Удалить
                    </button>
                </li>
            </ul>

            <h2>Сводка за день</h2>
            <div class="row">
                <input type="date" v-model="summaryDate" @change="run(loadSummary)" />
                <div v-if="summary" class="sticker" :class="stickerClass">
                    {{ summary.totalMinutes }} мин — {{ summary.stickerColor }}
                </div>
            </div>
        </section>
    </div>
</template>

<style scoped>
.page {
    max-width: 800px;
    margin: 0 auto;
    padding: 24px;
    font-family: sans-serif;
}
.tabs {
    display: flex;
    gap: 8px;
    margin-bottom: 16px;
}
.tabs button {
    padding: 8px 16px;
    border: 1px solid #ccc;
    background: #f5f5f5;
    cursor: pointer;
}
.tabs button.active {
    background: #42b883;
    border-color: #42b883;
    color: #fff;
}
.row {
    display: flex;
    gap: 8px;
    flex-wrap: wrap;
    align-items: center;
    margin-bottom: 16px;
}
.row input,
.row select {
    padding: 6px 8px;
    border: 1px solid #ccc;
    border-radius: 4px;
}
.row button {
    padding: 6px 12px;
    border: none;
    border-radius: 4px;
    background: #35495e;
    color: #fff;
    cursor: pointer;
}
.danger {
    padding: 4px 10px;
    border: none;
    border-radius: 4px;
    background: #e4606d;
    color: #fff;
    cursor: pointer;
    margin-left: 8px;
}
ul {
    list-style: none;
    padding: 0;
}
li {
    padding: 8px;
    border-bottom: 1px solid #eee;
}
.error {
    color: #e4606d;
    font-weight: bold;
}
.sticker {
    padding: 12px 20px;
    border-radius: 8px;
    font-weight: bold;
}
.sticker.yellow {
    background: #fde047;
}
.sticker.green {
    background: #86efac;
}
.sticker.red {
    background: #fca5a5;
}
</style>